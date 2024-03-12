const hljs = require('highlight.js');

hljs.listLanguages().forEach((lang) => {
    console.log(lang);
    hljs.getLanguage(lang).aliases?.forEach((alias) => console.log(alias));
});