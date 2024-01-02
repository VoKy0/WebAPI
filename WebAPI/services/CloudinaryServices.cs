using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;

namespace webapi_csharp.services
{
    

    public class CloudinaryServices
    {
        private readonly IConfiguration configuration;
        private readonly Account _account;
        private readonly Cloudinary _cloudinary;

        public CloudinaryServices()
        {
            var cloudName = configuration["DATABASE:CLOUDINARY:NAME"];
            var apiKey = configuration["DATABASE:CLOUDINARY:API_KEY"];
            var apiSecret = configuration["DATABASE:CLOUDINARY:API_SECRET"];

            _account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(_account);
        }
        public string uploadImage(string imagePath = "https://upload.wikimedia.org/wikipedia/commons/a/ae/Olympic_flag.jpg")
        {
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(imagePath),
                UseFilename = true,
                UniqueFilename = false,
                Overwrite = true,
            };

            try
            {
                var uploadResult = _cloudinary.Upload(uploadParams);
                Console.WriteLine(uploadResult);
                return uploadResult.PublicId;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public bool destroyImage(string publicId)
        {
            Console.WriteLine(publicId);
            try
            {
                var deletionParams = new DeletionParams(publicId);
                var deletionResult = _cloudinary.Destroy(deletionParams);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public string fetchUrl(string publicId)
        {
            var url = _cloudinary.Api.UrlImgUp.BuildUrl(publicId);
            return url;
        }
    }

}