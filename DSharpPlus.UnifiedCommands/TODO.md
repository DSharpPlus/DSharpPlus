# TODO

## Featurs
- [X] Implement the basic command handling
- [X] Implementing command parsing and converting.
- [X] Implementing middleware ~~(Can be improved)~~ (Is improved and called conditions)
- [X] Implement a default error handler for parsing/converting and allow the developer to change that.
- [X] Making tests in DSharpPlus.Test
- [X] Implement test for everything to make sure it works.
- [X] Spaces
- [X] Allow arguments to consume the entirety of a text. Like t!text hello there would return "hello there" to the argument with this marked attribute.
- [X] Allow quotes in text so above is not needed (all the time, above might be useful in some scenarios.).
- [ ] Overloading.

## Optimisation
- [ ] Better parsing
- [ ] Spans (Aki perf intensifies)
- [ ] Better parameter binding
- [ ] Make a better tree for command storing
- [ ] Better lookup for said tree. Preferably parent-child type of tree.

## Decisions
- Should we continue using DSharpPlus.Interactivity or should it be reimplemented into CommandModule?
- Should we call classes as `xCommand` (for example `MessageCommand`) or should we call it `x` (for example `Message`) 