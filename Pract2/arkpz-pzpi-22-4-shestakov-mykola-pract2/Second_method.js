//Поганий прикоад
class ShoppingCart {
    constructor() {
        this.items = [];
    }

    getTotalAndClear() {
        let total = 0;
        for (let item of this.items) {
            total += item.price * item.quantity;
        }
        this.items = [];
        return total;
    }

    addItem(item) {
        this.items.push(item);
    }
}

const cart = new ShoppingCart();
cart.addItem({ name: "Book", price: 20, quantity: 2 });
cart.addItem({ name: "Pen", price: 2, quantity: 5 });

const total = cart.getTotalAndClear();
console.log(`Total: $${total}`);
console.log(`Cart items: ${cart.items.length}`);


//Гарний приклад
class ShoppingCart {
    constructor() {
        this.items = [];
    }

    getTotal() {
        let total = 0;
        for (let item of this.items) {
            total += item.price * item.quantity;
        }
        return total;
    }

    clearCart() {
        this.items = [];
    }

    addItem(item) {
        this.items.push(item);
    }
}

const cart = new ShoppingCart();
cart.addItem({ name: "Book", price: 20, quantity: 2 });
cart.addItem({ name: "Pen", price: 2, quantity: 5 });

const total = cart.getTotal();
cart.clearCart();

console.log(`Total: $${total}`);
console.log(`Cart items: ${cart.items.length}`);
