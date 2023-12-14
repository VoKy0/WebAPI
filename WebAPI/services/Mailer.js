const config = require('config');
const nodemailer = require("nodemailer");

class Mailer {
    #transporter;
    #service;
    constructor(service="HOSTINGER") {
        this.#service = service;
        this.#transporter = nodemailer.createTransport({
            host: config.get(`MAILER.${service}.HOST`),
            port: config.get(`MAILER.${service}.PORT`),
            secure: true,
            auth: {
                user: config.get(`PRIVATE_INFORM.EMAIL.${service}.ADDRESS`),
                pass: config.get(`PRIVATE_INFORM.EMAIL.${service}.PASSWORD`),
            },
        });
    }

    async sendMailAsync(recipient_email, subject, content) {

        const mail_configs = {
            from: config.get(`PRIVATE_INFORM.EMAIL.${this.#service}.ADDRESS`),
            to: recipient_email,
            subject: subject,
            html: content,
        };
        return new Promise((resolve, reject) => {
            this.#transporter.sendMail(mail_configs, function (error, info) {
                if (error) {
                    reject(error);
                } else {
                    resolve('Mail sent')
                }
            });
        })

    }
}

module.exports = Mailer;