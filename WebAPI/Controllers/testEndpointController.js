const http = require('http');
const {HTTPSnippet} = require("httpsnippet");


const testEndpoint = (req, res) => {
    try {
        const options = {
            method: req.body.method,
            hostname: req.body.host,
            path: `${req.body.path}${req.body.queries.length > 0? '?' + req.body.queries.map(item => `${encodeURIComponent(item.name)}=${encodeURIComponent(item.value)}`).join('&'): ''}`,
            headers: req.body.headers.reduce((obj, value, index) => {
                obj[value.name] = value.value;
                return obj;
            }, {}),
        };
        options.headers['x-softzoneapi-key'] = req.body.access_token;
        const request = http.request(options, function (response) {
            const chunks = [];

            response.on('data', function (chunk) {
                chunks.push(chunk);
            });

            response.on('end', function () {
                const body = Buffer.concat(chunks);
                console.log('body', body.toString());
                res.status(200).json({
                    "success": true,
                    "message": 'Endpoint tested',
                    "data": [{response: body.toString()}]
                })
            });
        });
        console.log(req.body.body.value);
        request.write(JSON.stringify(JSON.parse(req.body.body.value)));
        request.end();
    } catch (err) {
        console.log('err', err)
        res.status(200).json({"success": false, "message": err.message, "data": []})
    }
}

module.exports = {
    testEndpoint
}