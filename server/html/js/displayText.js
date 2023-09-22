//========================== Text class ==========================
class DisplayText {
    /**
     * 
     * @param {string} text the text to display
     * @param {int} r 0-255 color
     * @param {int} g 0-255 color
     * @param {int} b 0-255 color
     * @param {double} fade <= 1 how much to fade the text
     */
    constructor(text, fontsize, r, g, b, fade) {
        this.text = text;
        this.fontsize = fontsize;
        this.r = r;
        this.g = g;
        this.b = b;
        this.fade = fade;
    }

    draw(c, x, y) {
        c.font = this.fontsize + 'px Comic Sans MS';
        c.textAlign = "center";
        c.fillStyle = 'rgba(' + this.r + ', ' + this.g + ', ' + this.b + ', ' + this.fade + ')';
        c.fillText(this.text, x, y);
    }
}