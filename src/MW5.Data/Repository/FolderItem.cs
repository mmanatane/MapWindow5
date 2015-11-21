﻿using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MW5.Api.Concrete;
using MW5.Api.Enums;
using MW5.Data.Enums;
using MW5.Shared;
using Syncfusion.Windows.Forms.Tools;

namespace MW5.Data.Repository
{
    internal class FolderItem : MetadataItem<FolderItemMetadata>, IFolderItem
    {
        // generated by DriverManager.DumpExtensions
        private const string VectorFormats = "000|bna|csv|dat|dgn|dxf|e00|gdb|gml|gmt|gpkg|gpx|jml|kml|map|mdb|mdb|ods|pix|rec|shp|sql|sqlite|svg|sxf|thf|vct|vfk|vrt|xlsx|xml";
        private const string RasterFormats = "ACE2|asc|bin|blx|bmp|bt|dat|ddf|dem|e00|ecw|gen|gff|gif|gif|gpkg|grb|grc|grd|grd|grd|grd|gsb|gtx|gxf|hdf5|hdr|hdr|hdr|hf2|hgt|img|img|jp2|jp2|jpg|kro|lcp|map|mbtiles|mem|mpr/mpl|n1|nat|nc|nc|ntf|pix|png|pnm|ppi|rda|rgb|rik|rst|rsw|sdat|sid|ter|ter|tif|toc|vrt|xml|xpm|xyz";
        
        // other extensions that for some reason aren't reported by DriverManager
        private const string OtherRasterFormats = "grib2";
        private const string OtherVectorFormats = "";

        // formats we typically don't want to show, like XML with metadata
        private const string UnwantedFormats = "xml";

        // old MapWinGIS list of extensions
        private const string ImageFormats = "tif|png|hdr.adf|asc|bt|bil|bmp|dem|ecw|img|gif|map|jp2|jpg|sid|pgm|pnm|png|ppm|vrt|tif|ntf";
        private const string GridFormats = "sta.adf|bgd|asc|tif|cel0.ddf|arc|aux|pix|dem|dhm|dt0|img|dt1|bil|nc";

        private const string SearchRegex = @"$(?<=\.({0}))";
        
        private static readonly Regex VectorRegex = new Regex(GetSearchRegex(FormatType.Vector), RegexOptions.IgnoreCase);
        private static readonly Regex ImageRegex = new Regex(GetSearchRegex(FormatType.Raster), RegexOptions.IgnoreCase);
        private static readonly Regex GridRegex = new Regex(GetSearchRegex(FormatType.Grid), RegexOptions.IgnoreCase);

        public FolderItem(TreeNodeAdv node) : base(node)
        {
            
        }

        public bool Expanded
        {
            get { return _node.Expanded; }
        }

        public string GetPath()
        {
            return Metadata.Path;
        }

        public bool Root
        {
            get { return Metadata.Root; }
        }

        public void ForceExpand()
        {
            ExpandCore();
            _node.Expanded = true;
        }

        private void ExpandCore()
        {
            string root = GetPath();
            var items = SubItems;

            try
            {
                EnumerateFolders(root, items);

                EnumerateFiles(root, items);
            }
            catch (Exception ex)
            {
                Logger.Current.Warn("Failed to enumerate contents of the folder in the repository: " + root, ex);
            }

            _node.ExpandedOnce = true;
        }

        public override void Expand()
        {
            if (_node.ExpandedOnce) return;

            ForceExpand();
        }

        private void EnumerateFolders(string root, RepositoryItemCollection items)
        {
            foreach (var path in Directory.EnumerateDirectories(root))
            {
                var info = new DirectoryInfo(path);

                if (!info.Attributes.HasFlag(FileAttributes.Hidden))
                {
                    items.AddFolder(path, false);
                }
            }
        }

        private void EnumerateFiles(string root, RepositoryItemCollection items)
        {
            var pattern = new Regex(GetSearchRegex(FormatType.All), RegexOptions.IgnoreCase);
            var files = Directory.EnumerateFiles(root).Where(f => pattern.IsMatch(f));

            foreach (var f in files)
            {
                var info = new FileInfo(f);
                if (info.Attributes.HasFlag(FileAttributes.Hidden))
                {
                    continue;
                }

                if (VectorRegex.IsMatch(f))
                {
                    items.AddFileVector(f);
                    continue;
                }

                items.AddFileImage(f);
            }
        }

        private static string GetExtensionList(FormatType format)
        {
            string s = string.Empty;

            switch (format)
            {
                case FormatType.All:
                    s = VectorFormats + "|" + RasterFormats + "|" + OtherVectorFormats + "|" + OtherRasterFormats;
                    break;
                case FormatType.Vector:
                    s = VectorFormats + OtherVectorFormats;
                    break;
                case FormatType.Raster:
                    s = ImageFormats;
                    break;
                case FormatType.Grid:
                    s = GridFormats;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("format");
            }

            return RemoveUnwantedFormats(s);
        }

        private static string RemoveUnwantedFormats(string s)
        {
            var items = UnwantedFormats.Split('|');
            foreach (var item in items)
            {
                s = s.Replace(item + "|", "");
                s = s.Replace( "|" + item, "");
            }

            return s;
        }

        private static string GetSearchRegex(FormatType format)
        {
            return string.Format(SearchRegex, GetExtensionList(format));
        }

        public bool ExpandedOnce
        {
            get { return _node.ExpandedOnce; }
        }

        public bool IsParentOf(LayerIdentity identity)
        {
            if (identity.IdentityType != LayerIdentityType.File)
            {
                return false;
            }

            return Shared.PathHelper.IsParentOf(GetPath(), identity.Filename);
        }
    }
}
