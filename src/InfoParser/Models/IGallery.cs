using System;
using System.Collections.Generic;
using SharedModel;

namespace InfoParser.Models
{
    public interface IGallery
    {
        int Id { get; set; }

        GalleryCategory Category { get; set; }

        int FileCount { get; set; }

        long FileSize { get; set; }

        bool IsExpunged { get; set; }

        long PostedDate { get; set; }

        DateTime PostedDateTime { get; set; }

        double Rating { get; set; }

        IEnumerable<Tag> Tags { get; }

        string Title { get; set; }

        string TitleJpn { get; set; }

        string Uploader { get; set; }

        string Url { get; set; }
    }
}