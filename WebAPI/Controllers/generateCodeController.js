const { HTTPSnippet } =require('httpsnippet');

const generateCode = (req, res) => {
    try {
        const snippet = new HTTPSnippet({
            method: req.query.method,
            url: 'http://localhost:2205',//req.query.url,
            headers: req.query.headers,
            queryString: req.query.queries,
            postData: {
                "mimeType": req.query.body.type,
                "jsonObj" : JSON.parse(req.query.body.value),
            }
        });
        const output = snippet.convert(req.query.lang);
        res.status(200).json({"success": true, "message": "Code successfully generated.", "data": [output]})
    } catch (err) {
        res.status(200).json({"success": false, "message": err.message, "data": []})
    }
}

module.exports = {
    generateCode
}