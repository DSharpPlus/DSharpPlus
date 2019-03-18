// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. See LICENSE file in the project root for full license information.

var Reset = "\x1b[0m"
var Bright = "\x1b[1m"
var Dim = "\x1b[2m"
var Underscore = "\x1b[4m"
var Blink = "\x1b[5m"
var Reverse = "\x1b[7m"
var Hidden = "\x1b[8m"

var FgBlack = "\x1b[30m"
var FgRed = "\x1b[31m"
var FgGreen = "\x1b[32m"
var FgYellow = "\x1b[33m"
var FgBlue = "\x1b[34m"
var FgMagenta = "\x1b[35m"
var FgCyan = "\x1b[36m"
var FgWhite = "\x1b[37m"

var BgBlack = "\x1b[40m"
var BgRed = "\x1b[41m"
var BgGreen = "\x1b[42m"
var BgYellow = "\x1b[43m"
var BgBlue = "\x1b[44m"
var BgMagenta = "\x1b[45m"
var BgCyan = "\x1b[46m"
var BgWhite = "\x1b[47m"

/**
 * This method will be called at the start of exports.transform in ManagedReference.html.primary.js
 */
exports.preTransform = function (model) {
  return model;
}

/**
 * This method will be called at the end of exports.transform in ManagedReference.html.primary.js
 */
exports.postTransform = function (model) {
  try {
    console.log('hi model: ' + model.type);
    
    model.aliases = [];
    handleItem(model, model.aliases);
    if (model.children) {
      model.children.forEach(function(item) {
        handleItem(item, model.aliases);
      });
    }
    
  } catch (e) {
    console.log(Bright + FgRed + '\nFail: ' + e + ',' + Object.keys(e) + Reset);
  }

  return model;
}

function handleItem(item, aliases) {
  //console.log('item: ' + JSON.stringify(item));
  if (item.remarks) {
    console.log('Remarks: ' + item.remarks);
    item.remarks = item.remarks.replace(/\[alias=(['"]|&quot;)(.*?)\1]/g, function($$, $quot, $aliasName) {
      console.log($$);
      console.log($quot);
      console.log($aliasName);
      aliases.push({
        isAlias: true,
        name: item.name[0].value.replace(/^.*?(\(|$)/, $aliasName + '$1'),
        id: $$,
        aliasTo: item.uid,
        
        targetXref: item.specName[0].value
        //targetName: item.name[0].value,
        //targetFullName: item.fullName[0].value
      });
      return '';
    });
    if (item.remarks.trim().length == 0) item.remarks = null;
  }
}
