using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataAccess;
using DataAccess.Models;

namespace DodjiParser
{
    internal abstract class ParsingStateEngineBase
    {
        #region Fields
        
        protected readonly DataRepository _repository;
        protected readonly ParsersRepository _parsersRepository;
        protected readonly List<ParsingState> _enqueuedParsingStates = new List<ParsingState>();
        protected readonly Queue<ParsingState> _queue = new Queue<ParsingState>();
        private readonly SemaphoreSlim _queueSemaphore = new SemaphoreSlim(1, 1);

        #endregion Fields

        #region Constructors

        protected ParsingStateEngineBase(DataRepository repository, ParsersRepository parsersRepository)
        {
            _repository = repository;
            _parsersRepository = parsersRepository;
        }

        #endregion

        #region Private methods

        protected virtual async Task Initialize()
        {
            await UpdateStates();
        }

        public abstract Task UpdateStates();

        protected async Task LoadNewParsingStates(GalleryState state)
        {
            // TODO Error states

            var parsingStates = await _repository.GetParsingStates(x => x.State == state);

            lock (_queue)
            lock (_enqueuedParsingStates)
            {
                var newParsingStates = parsingStates
                    .Where(ps => _enqueuedParsingStates.All(eps => eps.GalleryId != ps.Gallery.Id))
                    .ToList();

                _enqueuedParsingStates.AddRange(newParsingStates);

                foreach (var newParsingState in newParsingStates)
                {
                    _queue.Enqueue(newParsingState);
                }
            }

            await RestartQueueProcess();
        }

        private async Task RestartQueueProcess()
        {
            if (!_queueSemaphore.Wait(0))
            {
                return;
            }

            try
            {
                bool whileFlag;
                lock (_queue)
                {
                    whileFlag = _queue.Any();
                }

                while (whileFlag)
                {
                    ParsingState currentParsingState;
                    lock (_queue)
                    {
                        currentParsingState = _queue.Dequeue();
                    }

                    try
                    {
                        await ProcessParsingState(currentParsingState);
                    }
                    catch (Exception ex)
                    {
                        await
                            _repository.SetParsingStatus(currentParsingState.Id,
                                GalleryState.SearchError,
                                ex.Message);
                        // TODO log
                    }
                    finally
                    {
                        lock (_enqueuedParsingStates)
                        {
                            _enqueuedParsingStates.Remove(
                                _enqueuedParsingStates.First(x => x.Id == currentParsingState.Id));
                        }
                    }

                    lock (_queue)
                    {
                        whileFlag = _queue.Any();
                    }
                }
            }
            finally
            {
                _queueSemaphore.Release();
            }
        }

        protected abstract Task ProcessParsingState(ParsingState currentParsingState);

        #endregion


        public event EventHandler ParsingStateUpdated;

        protected void OnParsingStateUpdated()
        {
            ParsingStateUpdated?.Invoke(this, EventArgs.Empty);
        }
    }
}