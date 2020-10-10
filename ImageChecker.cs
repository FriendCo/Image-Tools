using Microsoft.AspNetCore.Http;
using System;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;

namespace Tools.ImageTools
{
    public static class ImageChecker
    {
        public const int ImageMinimumBytes = 512;
        public static bool IsImage(this IFormFile formFile)
        {
            //-------------------------------------------
            //  Check the image mime types
            //-------------------------------------------
            if (formFile.ContentType.ToLower() != "image/jpg" && formFile.ContentType.ToLower() != "image/png" && formFile.ContentType.ToLower() != "image/jpeg" &&
                formFile.ContentType.ToLower() != "image/pjpeg" &&
                formFile.ContentType.ToLower() != "image/gif" &&
                formFile.ContentType.ToLower() != "image/x-png" &&
                formFile.ContentType.ToLower() != "image/png")
            {
                return false;
            }


            //-------------------------------------------
            //  Check the image extension
            //-------------------------------------------
            if (Path.GetExtension(formFile.FileName).ToLower() != ".jpg" && Path.GetExtension(formFile.FileName).ToLower() != ".png"
                                                               && Path.GetExtension(formFile.FileName).ToLower() != ".gif"
                                                               && Path.GetExtension(formFile.FileName).ToLower() != ".jpeg")
            {
                return false;
            }

            //-------------------------------------------
            //  Attempt to read the file and check the first bytes
            //-------------------------------------------
            try
            {
                if (!formFile.OpenReadStream().CanRead)
                {
                    return false;
                }

                if (formFile.Length < ImageMinimumBytes)
                {
                    return false;
                }

                byte[] buffer = new Byte[512];
                formFile.OpenReadStream().Read(buffer, 0, 512);
                string content = System.Text.Encoding.UTF8.GetString(buffer);

                if (Regex.IsMatch(content, @"<script|<html|<head|<title|<body|<pre|<table|<a\s+href|<img|<plaintext|<cross\-domain\-policy",
                    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Multiline))
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }

            //-------------------------------------------
            //  Try to instantiate new Bitmap, if .NET will throw exception
            //  we can assume that it's not a valid image
            //-------------------------------------------

            try
            {
                using (var bitmap = new Bitmap(formFile.OpenReadStream()))
                {
                }
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
