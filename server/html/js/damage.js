// ====================================== Damages ======================================
/**
 * a Class to animate damage popping out of characters.
 */
class Damage {

    /**
     * animate damage. pops out from x,y and moves up and left or right while fading away.
     * 
     * @param {number} x x position on canvas
     * @param {number} y y position on canvas
     * @param {string} amount string to display
     * @param {number} r red value 0-255
     * @param {number} g green value 0-255
     * @param {number} b blue value 0-255
     */
    constructor(x, y, amount, r, g, b) {
        // set random amount of x
        this.directionX = randomInt(2, 4);
        // set direction of x. + or -
        this.xlr = 1;
        if (Math.random() > .5) {
            this.xlr = -1;
        }
        // set random amount of y moving up.
        this.directionY = -randomInt(3, 5);
        this.x = x;
        this.y = y;
        this.drawOrder = y
        this.r = r;
        this.g = g;
        this.b = b;
        this.amount = amount;
        // starting font size.
        this.fontSize = 8;
        // starting fade amount.
        this.fade = 1;
        // push the damage to the damage list for the world.
        damages.push(this);
    }

    /**
     * call to update the damage position and fade
     */
    update() {
        // update the position of the damage.
        this.directionX -= .5;
        if (this.directionX < 1) {
            this.directionX = 1;
        }
        this.directionY += .07;
        if (this.directionY > 2) {
            this.directionY = 2;
        }
        this.x += (this.directionX * this.xlr);
        this.y += this.directionY;
        this.fade -= .01;
        // remove damage.
        if (this.fade < 0) {
            const i = damages.indexOf(this);
            damages.splice(i, 1);
        }
        // grow the font for the next draw
        this.fontSize += .5;
        if (this.fontSize > 40) {
            this.fontSize = 40;
        }
    }

    /**
     * draw the damage to the canvas.
     * @param {number} x players x position on the canvas
     * @param {number} y players y position on the canvas
     */
    draw(c, x, y) {
        c.font = this.fontSize + 'px Comic Sans MS';
        c.textAlign = "center";
        c.fillStyle = 'rgba(' + this.r + ', ' + this.g + ', ' + this.b + ', ' + this.fade + ')';
        c.fillText(this.amount, this.x + x, this.y + y);
        this.update();
    }
}
