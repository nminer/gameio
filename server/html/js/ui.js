// ================================== ui ==============================================
class topbar {

    constructor() {
        this.image = ImageLoader.GetImage("./img/gui/playerBar.png");
        this.bag = ImageLoader.GetImage("./img/gui/bag.png"); //53 x59
        this.bagOpen = ImageLoader.GetImage("./img/gui/bagOpen.png"); // 52 x 54
        this.x = 5;
        this.y = 5;
        this.width = 389;
        this.height = 98;
    }

    draw(player) {
        //background
        c.fillStyle = 'rgba(60, 36, 21, 1)';
        c.fillRect(this.x + 50, this.y + 4, 330, 60);
        //health
        c.fillStyle = 'rgba(211, 0, 0, 1)';
        let hp = (273 * (player.health / player.maxHealth));
        c.fillRect(this.x + 102, this.y + 5, hp, 20);
        c.fillStyle = 'rgba(255, 255, 255, 0.2)';
        c.fillRect(this.x + 102, this.y + 10, 273, 2);
        c.fillStyle = 'rgba(0, 0, 0, 0.1)';
        c.fillRect(this.x + 102, this.y + 20, 273, 2);
        //stam
        c.fillStyle = 'rgba(255, 189, 36, 1)';
        let st = (273 * (player.stamina / player.maxStamina));
        c.fillRect(this.x + 102, this.y + 25, st, 20);
        c.fillStyle = 'rgba(255, 255, 255, 0.2)';
        c.fillRect(this.x + 102, this.y + 31, 273, 2);
        c.fillStyle = 'rgba(0, 0, 0, 0.1)';
        c.fillRect(this.x + 102, this.y + 41, 273, 2);
        //mana
        c.fillStyle = 'rgba(105, 0, 206, 1)';
        let mt = (273 * (player.mana / player.maxMana));
        c.fillRect(this.x + 102, this.y + 45, mt, 20);
        c.fillStyle = 'rgba(255, 255, 255, 0.2)';
        c.fillRect(this.x + 102, this.y + 51, 273, 2);
        c.fillStyle = 'rgba(0, 0, 0, 0.1)';
        c.fillRect(this.x + 102, this.y + 61, 273, 2);
        // ui player bar.
        c.drawImage(this.image,
            this.x,
            this.y,
            this.width,
            this.height);
        c.drawImage(this.bag,
            this.x + 23,
            this.y + 17,
            53,
            59);
        //c.drawImage(this.bagOpen,
        //    this.x + 25,
        //    this.y + 23,
        //    52,
        //    53);
        c.font = '14px Comic Sans MS';
        c.textAlign = "left";
        c.fillStyle = 'rgba(255, 255, 255, 0.7)';
        c.fillText(player.health + "/" + player.maxHealth, this.x + 110, this.y + 21);
        c.fillStyle = 'rgba(255, 255, 255, 0.7)';
        c.fillText(player.stamina + "/" + player.maxStamina, this.x + 110, this.y + 42);
        c.fillStyle = 'rgba(255, 255, 255, 0.7)';
        c.fillText(player.mana + "/" + player.maxMana, this.x + 110, this.y + 63);
        c.font = '12px Comic Sans MS';
        c.textAlign = "center";
        c.fillStyle = 'rgba(255, 255, 255, 0.4)';
        c.fillText("Health", this.x + 235, this.y + 21);
        c.fillText("Stamina", this.x + 235, this.y + 41);
        c.fillText("Mana", this.x + 235, this.y + 62);
    }
}