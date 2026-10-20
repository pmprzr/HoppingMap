function TSButton() {
    let name: string = "Fred";
    const element = document.getElementById("ts-example");
    if (element) {
        element.innerHTML = greeter(user);
    }
}

class Student {
    fullName: string;
    constructor(public firstName: string, public middleInitial: string, public lastName: string) {
        this.fullName = firstName + " " + middleInitial + " " + lastName;
    }
}

interface Person {
    firstName: string;
    lastName: string;
}

function greeter(person: Person) {
    return "Hello, " + person.firstName + " " + person.lastName;
}

let user = new Student("John", "M.", "Pork");