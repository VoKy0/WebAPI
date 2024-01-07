const Utils = require("../utils/Utils");
const RequestsModel = require("../models/Requests");

const getTopRequestsAPI = (req, res) => {
    const db = req.app.get('db');
    db.schema.hasTable("subscriptions").then(async function(exists) {
        if (exists) {
            try {
                db.select('s.*')
                    .count('r.* as request_count')
                    .from('services as s')
                    .join('subscriptions as su', 's.id', 'su.service_id')
                    .join('requests as r', 'r.subscription_id', 'su.id')
                    .groupBy('s.id')
                    .orderBy('request_count', 'desc')
                    .limit(req.query.limit).then(row => {
                        res.status(200).json({"success": true, "message": "Data successfully deleted from the database.", "data": row})
                });
            } catch(err) {
                console.log('error here: ', err.message);
                res.status(200).json({"success": false, "message": err.message, "data": []})
            }

        } else {
            res.status(200).json({"success": false, "message": "Table SUBSCRIPTIONS is not found!", "data": []});
        }
    })
}

module.exports = {
    getTopRequestsAPI
}