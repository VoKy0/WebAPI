const config = require('config');
const Redis = require('ioredis');
class RedisCluster {
    #cluster;

    constructor() {
        const nodes = config.get("DATABASE.REDIS_CLUSTER.NODES").map(node => ({
                host: node['HOST'],
                port: node['PORT'],
            }))
        this.#cluster = new Redis.Cluster(nodes, {
            // scaleReads: 'all',
            // slotsRefreshTimeout: 2000,
            redisOptions: {
                password: config.get("DATABASE.REDIS_CLUSTER.PASSWORD"),
            },
        });
    }
    async closeConnectionAsync() {
        await this.#cluster.quit();
    }
    getCluster() {
        return this.#cluster;
    }

}


module.exports = {
    RedisCluster
}