const { createClient } = require('redis');
const config = require('config');

class RedisServices {
    #client;

    constructor() {
        this.#client = createClient({
            // url: `redis://${config.get("DATABASE.REDIS.USER")}:${config.get("DATABASE.REDIS.PASSWORD")}@${config.get("DATABASE.REDIS.HOST")}:${config.get("DATABASE.REDIS.PORT")}`
            url: `redis://:${config.get("DATABASE.REDIS.PASSWORD")}@${config.get("DATABASE.REDIS.HOST")}:${config.get("DATABASE.REDIS.PORT")}`
        });
    }

    getClient() {
        return this.#client;
    }
    getValue(key) {
        return new Promise((resolve, reject) => {
            this.#client.get(key, (error, result) => {
                if (error) {
                    reject(error);
                } else {
                    resolve(result);
                }
            });
        });
    }

    setValue(key, value) {
        return new Promise((resolve, reject) => {
            this.#client.set(key, value, (error, result) => {
                if (error) {
                    reject(error);
                } else {
                    resolve(result);
                }
            });
        });
    }
    closeConnection() {
        this.#client.quit();
    }
    async connect() {
        await this.#client.connect();
        await this.#client.select( config.get("DATABASE.REDIS.DATABASE") );
        console.log("Connected!")
    }
}

async function connect() {
    const client = createClient({
        url: `redis://${config.get("DATABASE.REDIS.USER")}:${config.get("DATABASE.REDIS.PASSWORD")}@${config.get("DATABASE.REDIS.HOST")}:${config.get("DATABASE.REDIS.PORT")}`
    });

    client.on('error', err => console.log('Redis Client Error', err));

    // client.on('connect', () => {
    //     console.log('Connected!');
    // });
    await client.connect();
    await client.select( config.get("DATABASE.REDIS.DATABASE") );
    console.log("Connected!")
}


module.exports = {
    connect,
    RedisServices
}