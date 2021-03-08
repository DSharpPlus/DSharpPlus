---
uid: developer_guide_index
title: Developers Guide
---

# Intro
This guide is meant to serve as set of recommendations in if you are going to be contributing to the library,
how your code, PR, etc should be presented, formatted, etc.  This does not mean that we cannot deviate from normal 
process if a good reason why is provided.  

# When creating a PR
When creating a PR on github, please fill out all the details specified in the $(SolutionDir)\.github\PULL_REQUEST_TEMPLATE.md file.  This way 
a common standard can be used.  Also whenever possible, please have an issue created before submitting the PR and then link to that issue in the PR.

# When performing a review on a PR OR adding comments to an issue
When performing either one of these, please be curtious as possible.  NOW this does not mean that you must always use please and thank you (although it would be appreciated), being a 
dick trying to show off your e-peen will not be tolerated (Its sad we have to actually write this).  This will also be the same inside the Discord Support Server except in the dedicated shitposting channel.  

# When Creating a Builder
When creating a builder within the lib, we would like
1. To Keep a current uniform Method Names
2. Keep Creation/Modify seperate when certain properties cannot be done on one or the other endpoints.  Example would be on Channel create, u need to specify the type but on Modify, you cannot.
3. Methods that change a property should follow normal builder pattern vocabulary and use With and not Add.

## Builders that Interact with the API
Whenever possible, we should have a seperate Create and seperate Modify Builder but they should all inherit from the same base where common properties/methods are used between the two.  With that 
said, each builder should have a dedicated Clear and a dedicated internal validate method.  Specifically the internal validate method should always exist (and called) even if it is empty 
to help setup incase if we need to add validation in the future with minimal changes needed depending on Discords mood of the day.

## Builders that DONT Interact with the API
Whenever possible, we should have a dedicated Clear and a dedicated Build Method.  There does not have to be a dedicated Create/Modify builders as you should always assume that it will be used
in creation (example will be Embeds).