var AWS = require('aws-sdk');
var fs = require('fs');

AWS.config.update({ region: 'us-west-2' });

/* Synthesizes plain text or SSML into a file of human-like speech. */
var polly = new AWS.Polly({ apiVersion: "2016-06-10" });

var params = {
    OutputFormat: "mp3",
    SampleRate: "8000",
    Text: "All Gaul is divided into three parts",
    TextType: "text",
    VoiceId: "Joanna"
};

polly.synthesizeSpeech(params, function (err, data) {
    if (err) {
        console.log(err, err.stack);
    } else {
        console.log(data); 
        fs.writeFile('audio.mp3', data.AudioStream, (err) => {
            if (err) throw err;
            console.log('The file has been saved!');
        });
    }
});