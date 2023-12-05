const axios = require('axios');

const omitObjectKey = (object, key) => {
    const { [key]: omitted, ...rest } = object;
    return rest;
}

const redirectGetRequest = (req, res) => {
    const baseUrl = 'http://103.42.57.126'//req.app.get('base_url');
    const routePath = req.originalUrl;
    const url = new URL(routePath, baseUrl).href;
    console.log(url);
    axios.get(url, {
        headers: omitObjectKey(req.headers, 'x-softzoneapi-key'),
        params: req.query,
    })
        .then((response) => {
            res.status(200).json(response.data);
        })
        .catch((error) => {
            res.status(500).json({ error: `An error occurred while making the GET request: ${error.message}` });
        });
}

const redirectPostRequest = (req, res) => {
    const baseUrl = req.app.get('base_url');

    const routePath = req.originalUrl;
    const url = new URL(routePath, baseUrl).href

    axios.post(url, req.body, {
        headers: omitObjectKey(req.headers, 'x-softzoneapi-key'),
        params: req.query,
    })
        .then((response) => {
            res.status(200).json(response.data);
        })
        .catch((error) => {
            res.status(500).json({ error: `An error occurred while making the POST request: ${error.message}` });
        });
}

module.exports = {
    redirectGetRequest,
    redirectPostRequest
}