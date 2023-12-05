const {generatePrivateKey} = require('../services/appAuthServices');
const CloudinaryServices = require("../services/CloudinaryServices");
const Utils = require("../utils/Utils");
const cloudinaryProvider = new CloudinaryServices();

const getServices = (req, res) => {
    const db = req.app.get('db');
    db.schema.hasTable("services").then(function(exists) {
        if (exists) {
            db.select("*").from("services").then(rows =>
                res.status(200).json({"success": true, "message": "Data successfully queried from the database.", "data": rows})
            );

        } else {
            res.status(200).json({"success": false, "message": "Table SERVICES is not found!", "data": []});
        }
    })
}
const addService = (req, res) => {
    const db = req.app.get('db');
        db.schema.hasTable("services").then(async function(exists) {
            let service = {};
            if (exists) {
                try {
                    const avtPublicId = await cloudinaryProvider.uploadImage(req.body.image_data);
                    console.log('avtPublicId: ', avtPublicId)
                    db.transaction(function(trx) {
                        db.insert({
                            name: req.body.name,
                            description: req.body.description,
                            vendor_id: req.body.vendor_id,
                            visibility: req.body.visibility,
                            service_category_id: req.body.service_category_id,
                            avatar_public_id: avtPublicId
                        }, '*').into('services').transacting(trx).then(result => {
                            console.log(result, 'result')
                            const serviceId = result[0].id;
                            service = result[0];
                            const privateKey = generatePrivateKey(`${req.body.name}_${req.body.vendor_id}_${(new Date()).toString()}`);
                            const addPrivateKey = db("service_authentication").insert({
                                service_id: serviceId,
                                private_key: privateKey
                            }).transacting(trx);
                            return addPrivateKey;
                        }).then(trx.commit).catch(trx.rollback);
                    }).then(function(result) {
                        res.status(200).json({"success": true, "message": "Data successfully added to the database.", "data": [service]})
                    }).catch(function(error) {
                        console.log(error.message);
                        res.status(200).json({"success": false, "message": error.message, "data": []});
                    })
                } catch(err) {
                    console.log('error here: ', err.message);
                    res.status(500).json({"success": false, "message": err.message, "data": []})
                }

            } else {
                res.status(200).json({"success": false, "message": "Table SERVICES is not found!", "data": []});
            }
        })
}

const updateService = (req, res) => {
    const db = req.app.get('db');
    db.schema.hasTable("services").then(async function(exists) {
        if (exists) {
            try {
                const avtPublicId = await cloudinaryProvider.uploadImage(req.body.image_data);
                let updateData = {
                    name: req.body.name,
                    description: req.body.description,
                    vendor_id: req.body.vendor_id,
                    visibility: req.body.visibility,
                    service_category_id: req.body.service_category_id,
                    base_url: req.body.base_url,
                };
                if (avtPublicId) updateData["avatar_public_id"] = avtPublicId;
                db('services').where({id: req.body.id}).update(updateData).returning('*').then(row => {
                    res.status(200).json({
                        "success": true,
                        "message": "Data successfully updated to the database.",
                        "data": row
                    })
                })
            } catch(err) {
                console.log('error here: ', err.message);
                res.status(500).json({"success": false, "message": err.message, "data": []})
            }

        } else {
            res.status(200).json({"success": false, "message": "Table SERVICES is not found!", "data": []});
        }
    })
}
const getServiceById = (req, res) => {
    const db = req.app.get('db');
    db.schema.hasTable("services").then(function(exists) {
        if (exists) {
            db.select("*").from("services").where("id", req.query.id).then(rows =>
                res.status(200).json({"success": true, "message": "Data successfully queried from the database.", "data": rows})
            );

        } else {
            res.status(200).json({"success": false, "message": "Table SERVICES is not found!", "data": []});
        }
    })
}
const getServicesByServiceCategoryName = (req, res) => {
    const db = req.app.get('db');
    db.schema.hasTable("services").then(function(exists) {
        if (exists) {
            db.select("services.*").from("services").join('service_categories', 'services.service_category_id', '=', 'service_categories.id').where("service_categories.name", req.query.service_category_name).then(rows =>
                res.status(200).json({"success": true, "message": "Data successfully queried from the database.", "data": rows})
            );

        } else {
            res.status(200).json({"success": false, "message": "Table SERVICES is not found!", "data": []});
        }
    })
}

const getServicesByVendorId = (req, res) => {
    console.log('here get by vendor id')
    const db = req.app.get('db');
    db.schema.hasTable("services").then(function(exists) {
        if (exists) {
            db.select("*").from("services").where("vendor_id", req.query.vendor_id).then(rows =>
                res.status(200).json({"success": true, "message": "Data successfully queried from the database.", "data": rows})
            );

        } else {
            res.status(200).json({"success": false, "message": "Table SERVICES is not found!", "data": []});
        }
    })
}

const getServicesByServiceCategoryId = (req, res) => {
    const db = req.app.get('db');
    db.schema.hasTable("services").then(function(exists) {
        if (exists) {
            db.select("*").from("services").where("service_category_id", req.query.service_category_id).then(rows =>
                res.status(200).json({"success": true, "message": "Data successfully queried from the database.", "data": rows})
            );

        } else {
            res.status(200).json({"success": false, "message": "Table SERVICES is not found!", "data": []});
        }
    })
}
module.exports = {
    getServices,
    getServiceById,
    getServicesByServiceCategoryId,
    getServicesByServiceCategoryName,
    getServicesByVendorId,
    addService,
    updateService
}