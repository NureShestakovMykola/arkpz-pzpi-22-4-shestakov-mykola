// Поганий приклад
function x(a) {
    return a * 2;
}

// Гарний приклад
function multiplyByTwo(number) {
    return number * 2;
}

// Поганий приклад
function data() {
    return true;
}

// Гарний приклад
function fetchData() {
    return true;
}

// Поганий приклад
const user_data = {};
class userprofile {}

// Гарний приклад
const userData = {};
class UserProfile {}

// Поганий приклад
let x = 10;

// Гарний приклад
let studentCount = 10;

// Поганий приклад
if (x > 10){
console.log('More than 10');
}

// Гарний приклад
if (x > 10) {
    console.log('More than 10');
}

// Поганий приклад
if (x > 10) 
    console.log('More than 10');

// Гарний приклад
if (x > 10) {
    console.log('More than 10');
}

// Поганий приклад
function processOrder(order) {
    if (!order) return;
    if (!order.items.length) return;
    if (order.status !== 'completed') return;
    console.log('Processing order...');
}

// Гарний приклад
function validateOrder(order) {
    if (!order) throw new Error('Order is missing');
    if (!order.items.length) throw new Error('Order has no items');
    if (order.status !== 'completed') throw new Error('Order is not completed');
}

function processOrder(order) {
    validateOrder(order);
    console.log('Processing order...');
}

// Поганий приклад
const user = { name: 'John', age: 30 };
const name = user.name;
const age = user.age;

// Гарний приклад
const user = { name: 'John', age: 30 };
const { name, age } = user;

// Поганий приклад
const data = JSON.parse(userInput);

// Гарний приклад
try {
    const data = JSON.parse(userInput);
} catch (error) {
    console.error('Invalid JSON:', error);
}

// Гарний приклад
window.addEventListener('error', function(event) {
    console.error('Global error:', event.message);
});

// Поганий приклад
function square(x) {
    return x * x;
}

function cube(x) {
    return x * x * x;
}

// Гарний приклад
function power(x, n) {
    return Math.pow(x, n);
}

// Поганий приклад
if (data) {
    if (data.valid) {
        console.log('Valid data');
    }
}

// Гарний приклад
if (!data) return;
if (!data.valid) return;
console.log('Valid data');

// Поганий приклад
function processOrder(order) {
    if (!order) return;
    if (!order.items.length) return;
    console.log('Processing order...');
}

// Гарний приклад
function validateOrder(order) {
    if (!order) throw new Error('Order is missing');
    if (!order.items.length) throw new Error('Order has no items');
}

function processOrder(order) {
    validateOrder(order);
    console.log('Processing order...');
}

// Поганий приклад
if (user.age >= 18) {
    console.log('User is an adult');
}

// Гарний приклад
const ADULT_AGE = 18;
if (user.age >= ADULT_AGE) {
    console.log('User is an adult');
}

// Поганий приклад
function fetchData() {
    // Старий код, який більше не потрібний
    // const url = 'https://example.com';
    console.log('Data fetched');
}

// Гарний приклад
function fetchData() {
    console.log('Data fetched');
}

// Поганий приклад
var x = 10;
function sum(a, b) {
    return a + b;
}

// Гарний приклад
const x = 10;
const sum = (a, b) => a + b;

// Поганий приклад
const user = { name: 'John', age: 30 };
user.age = 31; // Зміна об'єкта

// Гарний приклад
const user = { name: 'John', age: 30 };
const updatedUser = { ...user, age: 31 }; // Новий об'єкт із оновленими даними

// Поганий приклад
const numbers = [1, 2, 3, 4];
const doubled = [];
for (let i = 0; i < numbers.length; i++) {
    doubled.push(numbers[i] * 2);
}

// Гарний приклад
const numbers = [1, 2, 3, 4];
const doubled = numbers.map(num => num * 2);

// Поганий приклад
function factorial(n) {
    if (n <= 1) return 1;
    return n * factorial(n - 1);
}

// Гарний приклад
const memoizedFactorial = (() => {
    const cache = {};
    return function factorial(n) {
        if (n in cache) return cache[n];
        if (n <= 1) return 1;
        cache[n] = n * factorial(n - 1);
        return cache[n];
    };
})();

// Поганий приклад
let x = 10; // Змінна для значення 10

// Гарний приклад
// Лічильник спроб підключення до сервера
let connectionAttempts = 10;

// Гарний приклад (тест для функції)
function sum(a, b) {
    return a + b;
}

// Тест для функції sum
console.assert(sum(2, 3) === 5, 'Test failed: sum(2, 3) should be 5');

// Поганий приклад
function processOrder(order) {
    if (!order) return;
    if (!order.items.length) return;
    sendEmail(order);
    console.log('Order processed');
}

// Гарний приклад
function validateOrder(order) {
    if (!order) throw new Error('Order is missing');
    if (!order.items.length) throw new Error('Order has no items');
}

function sendOrderConfirmation(order) {
    console.log('Order confirmation sent');
}

function processOrder(order) {
    validateOrder(order);
    sendOrderConfirmation(order);
}

// Поганий приклад
let total = 0;
function addToTotal(amount) {
    total += amount;
    return total;
}

// Гарний приклад
function calculateTotal(baseTotal, amount) {
    return baseTotal + amount;
}

// Поганий приклад
function addItemToList(item, list) {
    list.push(item);
}

// Гарний приклад
function addItemToList(item, list) {
    return [...list, item];
}
