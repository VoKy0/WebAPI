const { HTTPSnippet } =require('httpsnippet');
const generateCode = (req, res) => {
    try {
        let parsedBody = req.body.body.value;
        try {
            parsedBody = JSON.parse(parsedBody);
        } catch (e) {
            parsedBody = false;
        }
        const url = new URL(`http://${req.body.service_name.split(' ').map(word => word.charAt(0) + word.substring(1, word.length)).join('-')}.p.softzoneai.com`);
        url.pathname = req.body.route;
        const snippet = new HTTPSnippet({
            method: req.body.method,
            url: url.toString() ,//req.body.url,
            headers: req.body.headers,
            queryString: req.body.queries,
            postData: {
                "mimeType": req.body.body.type,
                "text": req.body.body.value,
                "jsonObj" : parsedBody,
            }

        });
        const output = snippet.convert(req.body.lang);
        res.status(200).json({"success": true, "message": "Code successfully generated.", "data": [output]})
    } catch (err) {
        console.log(err)
        res.status(200).json({"success": false, "message": err.message, "data": []})
    }
}

module.exports = {
    generateCode
}