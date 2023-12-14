const CloudinaryServices = require("../services/CloudinaryServices");

const cloudinaryProvider = new CloudinaryServices();

const uploadImage = async (req, res) => {
    try {
        const result = await cloudinaryProvider.uploadImage(req.body.image_data);
        res.status(200).json({
            "success": true,
            "message": "Data successfully added to the database.",
            "data": [result]
        })
    } catch(err) {
        console.log('error here: ', err.message);
        res.status(500).json({"success": false, "message": err.message, "data": []})
    }
}

const deleteImage = async (req, res) => {
    try {
        const isDestroyed = await cloudinaryProvider.destroyImage(req.body.public_id);
        if (isDestroyed) {
            res.status(200).json({
                "success": true,
                "message": "Image successfully deleted from cloudinary.",
                "data": []
            })
        } else {
            res.status(200).json({
                "success": false,
                "message": "Image cannot be deleted from cloudinary.",
                "data": []
            })
        }

    } catch(err) {
        console.log('error here: ', err.message);
        res.status(500).json({"success": false, "message": err.message, "data": []})
    }
}

const fetchImage = (req, res) => {
    try {
        const imageUrl = cloudinaryProvider.fetchUrl(req.query.public_id);
        res.status(200).json({"success": true, "message": "Fetch URL successfully", "data": [{'url': imageUrl}]})
    } catch (err) {
        res.status(500).json({"success": false, "message": err.message, "data": []})
    }
}

module.exports = {
    uploadImage,
    fetchImage,
    deleteImage
}