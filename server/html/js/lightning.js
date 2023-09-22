//================================= lightning ===========================
class Lightning {
    constructor(startingAmount = .8) {
        this.amount = startingAmount;
        this.finshed = false;
    }

    draw() {
        lighten(0, 0, canvas.width, canvas.height, '#ffffff', this.amount);
        this.amount = this.amount - 0.03;
        if (this.amount <= 0) {
            this.finshed = true;
        }
    }
}