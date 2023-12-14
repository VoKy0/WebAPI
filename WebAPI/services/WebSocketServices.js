const express = require('express');
const cors = require('cors');
const session = require('cookie-session');
const http = require('http');
const https = require('https');
const WebSocket = require('ws');
const fs = require('fs');

const app = express();
app.use(express.urlencoded({ extended: true }));
app.use(express.json());
app.use(cors());
app.use(session({
    resave: false,
    saveUninitialized: true,
    secret: "abc",
}));

const server = http.createServer(app);
// const server = https.createServer(app);
const wss = new WebSocket.Server({ server });

const createWebsocketServer = () => {
    wss.on('connection', (ws) => {
        console.log('WebSocket connection established.');
        ws.on('message', (messages) => {
            const data = messages.toString();
            const { event, message, attachment_public_id, discussion, comment } = JSON.parse(data);

            wss.clients.forEach((client) => {

                if(event === 'message'){
                    if (client !== ws && client.readyState === WebSocket.OPEN) {
                        const mes = {
                            action: 'message',
                            messages: message,
                            attachment_public_id: attachment_public_id,
                        };
                        client.send( JSON.stringify(mes))
                        console.log(`Received: ${message} ${attachment_public_id}`);
                    }
                } else if(event === 'discussion-tab'){
                    console.log('discussion:', discussion)
                    if (client !== ws && client.readyState === WebSocket.OPEN) {
                        const mes = {
                            action: 'discussion-tab',
                            discussion: discussion,
                        };
                        client.send( JSON.stringify(mes))
                        console.log(`Received: ${discussion} `);
                    }
                } else if(event === 'comment'){
                    console.log('comment:', comment)
                    if (client !== ws && client.readyState === WebSocket.OPEN) {
                        console.log('comment:', comment, 'opened')
                        const mes = {
                            action: 'comment',
                            comment: comment,
                        };
                        client.send( JSON.stringify(mes))
                        console.log(`Received: ${comment} `);
                    }
                } else if(event === 'delete-comment'){
                    console.log('comment:', comment)
                    if (client !== ws && client.readyState === WebSocket.OPEN) {
                        console.log('comment:', comment, 'opened')
                        const mes = {
                            action: 'delete-comment',
                            comment: comment,
                        };
                        client.send( JSON.stringify(mes))
                        console.log(`Received: ${comment} `);
                    }
                }
            });

        });

        ws.on('close', () => {

            console.log('WebSocket connection closed.');
        });
    });

    server.listen(2203, () =>{
        console.log('WebSocket server is listening on port 2203');
    });
};

const sendWSMessage = (message) => {
    console.log("sending message:", message);

    wss.clients.forEach((client) => {
        if (client.readyState === WebSocket.OPEN) {
            console.log('sending real')
            client.send( message)
        }
    })

}

module.exports = {
    createWebsocketServer,
    sendWSMessage
}

