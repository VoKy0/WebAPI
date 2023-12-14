const SubscriptionModel = require("../Models/SubscriptionsModel");
const cron = require('node-cron');
class SubscriptionJobScheduler {
    #jobs;
    #subscriptions;
    #db;

    constructor(db) {
        this.#jobs = {};
        this.#subscriptions = [];
        this.#db = db;
        this.initAsync();
    }
    createJobScheduler(id) {
        this.#jobs[id] = cron.schedule('* * * * *', async () => {
            // console.log(`Running job check subscription ${id}...`);

            try {
                const result = await SubscriptionModel.expiredById(this.#db, id);
                // console.log(result);
                if (result) {
                    // console.log(`Updated subscriptions ${id} expired status. Job killed.`);
                    this.#jobs[id].stop();
                } else {
                    // console.log(`Updated subscriptions ${id} expired status failed. Continue scheduling...`);
                }
            } catch (error) {
                console.error('Error updating subscriptions:', error.message);
                this.#jobs[id].stop();
            }
        });
    }
    killJobScheduler(id) {
        if (id in this.#jobs) {
            this.#jobs[id].stop();
            console.log(`Job ${id} killed.`);
        } else {
            console.log(`Job ${id} not found.`);
        }
    }
    async initAsync() {
        this.#subscriptions = await SubscriptionModel.getActiveSubscriptions(this.#db);
        this.#subscriptions.forEach(subscription => {
            this.createJobScheduler(subscription.id);
        })
    }

}

module.exports = {SubscriptionJobScheduler};