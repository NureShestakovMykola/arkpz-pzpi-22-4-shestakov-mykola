//Поганий прикоад
function processOrder(order) {
    let discount = 0;

    if (order.customerType === "VIP" && order.totalAmount > 1000 && isHolidaySeason()) {
        discount = order.totalAmount * 0.2;
    } else if (order.customerType === "Regular" && order.totalAmount > 500 && order.isFirstOrder) {
        discount = order.totalAmount * 0.1;
    } else if (order.customerType === "New" && order.totalAmount > 300) {
        discount = order.totalAmount * 0.05;
    }

    if (order.paymentMethod === "CreditCard" && order.totalAmount > 2000) {
        discount += 50;
    } else if (order.paymentMethod === "BankTransfer" && order.totalAmount > 1500) {
        discount += 30;
    }

    if (order.totalAmount - discount < 0) {
        return { error: "Discount exceeds total amount" };
    }

    return {
        finalAmount: order.totalAmount - discount,
        appliedDiscount: discount,
    };
}

function isHolidaySeason() {
    const today = new Date();
    return today.getMonth() === 11;
}


//Гарний приклад
function processOrder(order) {
    const discount = calculateDiscount(order);
    const bonusDiscount = calculateBonusDiscount(order);

    const totalDiscount = discount + bonusDiscount;

    if (order.totalAmount - totalDiscount < 0) {
        return { error: "Discount exceeds total amount" };
    }

    return {
        finalAmount: order.totalAmount - totalDiscount,
        appliedDiscount: totalDiscount,
    };
}

function calculateDiscount(order) {
    if (isVipEligibleForDiscount(order)) {
        return order.totalAmount * 0.2;
    } else if (isRegularEligibleForDiscount(order)) {
        return order.totalAmount * 0.1;
    } else if (isNewCustomerEligibleForDiscount(order)) {
        return order.totalAmount * 0.05;
    }
    return 0;
}

function calculateBonusDiscount(order) {
    if (isCreditCardBonusApplicable(order)) {
        return 50;
    } else if (isBankTransferBonusApplicable(order)) {
        return 30;
    }
    return 0;
}

function isVipEligibleForDiscount(order) {
    return order.customerType === "VIP" && order.totalAmount > 1000 && isHolidaySeason();
}

function isRegularEligibleForDiscount(order) {
    return order.customerType === "Regular" && order.totalAmount > 500 && order.isFirstOrder;
}

function isNewCustomerEligibleForDiscount(order) {
    return order.customerType === "New" && order.totalAmount > 300;
}

function isCreditCardBonusApplicable(order) {
    return order.paymentMethod === "CreditCard" && order.totalAmount > 2000;
}

function isBankTransferBonusApplicable(order) {
    return order.paymentMethod === "BankTransfer" && order.totalAmount > 1500;
}

function isHolidaySeason() {
    const today = new Date();
    return today.getMonth() === 11;
}
