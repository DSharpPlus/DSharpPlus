// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
    console.log('\nhi model: ' + model.type);
    
    model.aliases = [];
    handleItem(model, model.aliases);
    if (model.children) {
      model.children.forEach(function(item) {
        handleItem(item, model.aliases);
      });
    }
    
  } catch (e) {
    console.log('\nFail: ' + e);
  }

  return model;
}

function handleItem(item, aliases) {
  if (item.remarks) {
    item.remarks = item.remarks.replace(/\[alias=(['"])(.*?)\1]/g, function($$) {
      console.log($$);
      aliases.push({
        isAlias: true,
        name: item.name.replace(),
        id: $$,
        aliasTo: item.uid,
        targetName: item.name,
        targetFullName: item.fullName
      });
      return '';
    });
    if (item.remarks.trim().length == 0) item.remarks = null;
  }
}
