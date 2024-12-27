//Поганий прикоад
class Project {
    constructor() {
        this.tasks = [];
    }

    addTask(task) {
        this.tasks.push(task);
    }

    getTasks() {
        return this.tasks;
    }

    completeTask(taskId) {
        const task = this.tasks.find(task => task.id === taskId);
        if (task) {
            task.completed = true;
        }
    }

    clearCompletedTasks() {
        this.tasks = this.tasks.filter(task => !task.completed);
    }
}

const project = new Project();
project.addTask({ id: 1, name: "Design UI", completed: false });
project.addTask({ id: 2, name: "Write Documentation", completed: false });
project.addTask({ id: 3, name: "Implement Backend", completed: true });

project.completeTask(2);

console.log("Tasks before clearing completed:");
console.log(project.tasks);

project.clearCompletedTasks();

console.log("Tasks after clearing completed:");
console.log(project.tasks);



//Гарний приклад
class Project {
    constructor() {
        this._tasks = [];
    }

    addTask(task) {
        if (!task.id || !task.name) {
            throw new Error("Invalid task data");
        }
        this._tasks.push({ ...task, completed: task.completed || false });
    }

    getTasks() {
        return [...this._tasks];
    }

    completeTask(taskId) {
        const index = this._tasks.findIndex(task => task.id === taskId);
        if (index !== -1) {
            this._tasks[index].completed = true;
        }
    }

    removeTask(taskId) {
        const index = this._tasks.findIndex(task => task.id === taskId);
        if (index !== -1) {
            this._tasks.splice(index, 1);
        }
    }

    clearCompletedTasks() {
        this._tasks = this._tasks.filter(task => !task.completed);
    }
}

const project = new Project();
project.addTask({ id: 1, name: "Design UI" });
project.addTask({ id: 2, name: "Write Documentation" });
project.addTask({ id: 3, name: "Implement Backend", completed: true });

project.completeTask(2);

console.log("Tasks before clearing completed:");
console.log(project.getTasks());

project.clearCompletedTasks();

console.log("Tasks after clearing completed:");
console.log(project.getTasks());
