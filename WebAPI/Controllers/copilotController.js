const http = require('http');

const getSimilarAPIs = (req, res) => {
    try {
        const options = {
            method: 'post',
            hostname: '127.0.0.1',
            port: '3112',
            path: '/similarity_searching',
            headers: {
                'Content-Type': 'application/json'
            },
        };
        const request = http.request(options, function (response) {
            const chunks = [];

            response.on('data', function (chunk) {
                chunks.push(chunk);
            });

            response.on('end', function () {
                const body = Buffer.concat(chunks);
                console.log(body.toString())
                res.status(200).json({
                    "success": true,
                    "message": 'Success',
                    "data": JSON.parse(body.toString())
                })
            });
        });
        request.write(JSON.stringify({
            'target': req.body.input,
            'db': req.body.services
        }));
        request.end();
    } catch(err) {
        console.log('err', err)
        res.status(200).json({"success": false, "message": err.message, "data": []})
    }

}
module.exports = {
    getSimilarAPIs
}