using System;
using System.Runtime.Serialization;

namespace InfoParser.Models.JSON
{
    [Flags]
    public enum GalleryCategory
    {
        None                                            = 0,

        [EnumMember(Value = "Doujinshi")]   
        Doujinshi                                       = 1 << 0,

        [EnumMember(Value = "Manga")]
        Manga                                           = 1 << 1,

        [EnumMember(Value = "Artist CG Sets")]
        ArtistCgSets                                    = 1 << 2,

        [EnumMember(Value = "Game CG Sets")]
        GameCgSets                                      = 1 << 3,

        [EnumMember(Value = "Western")]
        Western                                         = 1 << 4,

        [EnumMember(Value = "Image Sets")]
        ImageSets                                       = 1 << 5,

        [EnumMember(Value = "Non-H")]
        NonH                                            = 1 << 6,

        [EnumMember(Value = "Cosplay")]
        Cosplay                                         = 1 << 7,

        [EnumMember(Value = "Asian Porn")]
        AsianPorn                                       = 1 << 8,

        [EnumMember(Value = "Misc")]
        Misc                                            = 1 << 9,

        [EnumMember(Value = "Private")]
        Private                                         = 1 << 10,

        All                                             = 2047
    }
}