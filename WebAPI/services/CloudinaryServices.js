const cloudinary = require('cloudinary').v2;

const config = require('config');

class CloudinaryServices {
    constructor() {
        cloudinary.config({
            cloud_name: config.get("DATABASE.CLOUDINARY.NAME"),
            api_key: config.get("DATABASE.CLOUDINARY.API_KEY"),
            api_secret: config.get("DATABASE.CLOUDINARY.API_SECRET")
        });
    }

    async uploadImage(imagePath="https://upload.wikimedia.org/wikipedia/commons/a/ae/Olympic_flag.jpg") {
        const options = {
            use_filename: true,
            unique_filename: false,
            overwrite: true,
        };

        try {
            // Upload the image
            const result = await cloudinary.uploader.upload(imagePath, options);
            console.log(result)
            return result.public_id;
        } catch (error) {
            console.error(error);
            return null;
        }
    }

    async destroyImage(publicId) {
        console.log(publicId)
        try {
            await cloudinary.uploader.destroy(publicId);
            return true;
        } catch(err) {
            console.log(err)
            return false;
        }

    }

    fetchUrl(publicId) {
        const url = cloudinary.url(publicId);
        return url;
    }
}

module.exports = CloudinaryServices;
