const express = require('express');
const app = express();
const port = 3006;

// Define a simple endpoint
app.get('/test', (req, res) => {
    res.json({ message: 'This is a test API endpoint.' });
});

// Start the server
app.listen(port, () => {
    console.log(`Server is running on http://localhost:${port}`);
});
