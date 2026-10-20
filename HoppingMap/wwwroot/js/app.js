"use strict";
function TSButton() {
    let name = "Fred";
    const element = document.getElementById("ts-example");
    if (element) {
        element.innerHTML = greeter(user);
    }
}
class Student {
    constructor(firstName, middleInitial, lastName) {
        this.firstName = firstName;
        this.middleInitial = middleInitial;
        this.lastName = lastName;
        this.fullName = firstName + " " + middleInitial + " " + lastName;
    }
}
function greeter(person) {
    return "Hello, " + person.firstName + " " + person.lastName;
}
let user = new Student("John", "M.", "Pork");
//# sourceMappingURL=app.js.map