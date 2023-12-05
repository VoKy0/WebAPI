const crypto = require('crypto');
const jwt = require('jsonwebtoken');

function generatePrivateKey(passphrase) {
    // Define the key generation options
    const keyOptions = {
        modulusLength: 1024, // Key length (e.g., 2048, 3072, or 4096 bits)
        privateKeyEncoding: {
            type: 'pkcs8',    // Private Key format
            format: 'pem'      // PEM encoding
        },
        privateKeyEncoding: {
            type: 'pkcs8',      // Private Key format
            format: 'pem',       // PEM encoding
            cipher: 'aes-256-cbc', // Optional encryption algorithm and passphrase
            passphrase: passphrase // Replace with your passphrase or remove this line
        }
    };


    // Generate the private key
    const privateKey = crypto.generateKeyPairSync('rsa', keyOptions).privateKey;
    return privateKey;
}

function generatePublicKeyFromPrivateKey(privateKeyPEM, interval) {
    try {
        const payload = {
            apiKey: 'your-api-key', // Replace with a unique API key identifier
        };

        // Sign the JWT using your private key
        const publicKey = jwt.sign(payload, privateKeyPEM, { expiresIn: `${interval != 'monthly'? `1${interval.charAt(0)}` : "30d"}`});
        return publicKey;
    } catch (error) {
        console.error('Error generating public key from private key:', error);
        return null;
    }
}

module.exports = {
    generatePrivateKey,
    generatePublicKeyFromPrivateKey
}