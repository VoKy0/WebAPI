const axios = require('axios');

// Your API endpoint
const apiUrl = 'http://test-api.p.softzoneai.com/test';

// Number of concurrent requests
const numberOfRequests = 100;
const headers = {
    'x-softzoneapi-key': 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJfaWQiOjEyNSwic3RhcnRBdCI6IjIwMjMtMTItMTlUMDQ6MTU6MTguNTA4WiIsImlhdCI6MTcwMjk1OTMxOCwiZXhwIjoxNzA1NTUxMzE4fQ.kFTBCSLY4RhC6pBEpOU82qyYWWPHr9S8EbDukJIl64w'
}
// Function to make a single API request
async function makeApiRequest() {

    try {
        const startTime = Date.now();
        const response = await axios.get(apiUrl, {headers})
        const endTime = Date.now();
        const duration = endTime - startTime;
        console.log(`Request success [Duration: ${duration} ms]:`, response.data);


    } catch (error) {
        console.error('Request failed:', error.message);
    }
}

// Create an array of promises for concurrent requests
const requests = Array.from({ length: numberOfRequests }, () => makeApiRequest());

// Measure the time taken for all requests
const startTime = Date.now();

// Execute all requests concurrently
Promise.all(requests)
    .then(() => {
        const endTime = Date.now();
        const duration = endTime - startTime;
        console.log(`All requests completed in ${duration} milliseconds.`);
    })
    .catch((error) => console.error('Promise.all failed:', error));
