const CloudinaryServices = require("../services/CloudinaryServices");
const cloudinaryProvider = new CloudinaryServices();

const getUserById = (req, res) => {
    console.log('id', req.query.id)
    const db = req.app.get('db');
    db.schema.hasTable("users").then(function(exists) {
        if (exists) {
            db.select("*").from("users").where("id", req.query.id).then(rows =>
                res.status(200).json({"success": true, "message": "Data successfully queried from the database.", "data": rows})
            );

        } else {
            res.status(200).json({"success": false, "message": "Table USERS is not found!", "data": []});
        }
    })
}
const getUserByUsername = (req, res) => {
    const db = req.app.get('db');
    console.log(req.query.username)
    db.schema.hasTable("users").then(function(exists) {
        if (exists) {
            db.select("*").from("users").where("username", req.query.username).then(rows =>
                res.status(200).json({"success": true, "message": "Data successfully queried from the database.", "data": rows})
            );

        } else {
            res.status(200).json({"success": false, "message": "Table USERS is not found!", "data": []});
        }
    })
}
const getUserByEmail = (req, res) => {
    const db = req.app.get('db');
    db.schema.hasTable("users").then(function(exists) {
        if (exists) {
            db.select("*").from("users").where("email", req.query.email).then(rows =>
                res.status(200).json({"success": true, "message": "Data successfully queried from the database.", "data": rows})
            );

        } else {
            res.status(200).json({"success": false, "message": "Table USERS is not found!", "data": []});
        }
    })
}

const getUsers = (req, res) => {
    const db = req.app.get('db');
    db.schema.hasTable("users").then(function(exists) {
        if (exists) {
            db.select("*").from("users").then(rows =>
                res.status(200).json({"success": true, "message": "Data successfully queried from the database.", "data": rows})
            );

        } else {
            res.status(200).json({"success": false, "message": "Table USERS is not found!", "data": []});
        }
    })
}
const updateUser = (req, res) => {
    const db = req.app.get('db');
    db.schema.hasTable("users").then(async function(exists) {
        if (exists) {
            const avtPublicId = await cloudinaryProvider.uploadImage(req.body.image_data);
            db("users").where({id: req.body.id}).update({
                first_name: req.body.first_name,
                last_name: req.body.last_name,
                avatar_public_id: avtPublicId
            }).returning('*').then(row => {
                res.status(200).json({
                    "success": true,
                    "message": "Data successfully updated to the database.",
                    "data": row
                })
            })
        } else {
            res.status(200).json({"success": false, "message": "Table USERS is not found!", "data": []});
        }
    })
}
module.exports = {
    getUserById,
    getUsers,
    getUserByUsername,
    updateUser
}