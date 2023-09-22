//=================================== storm ===========================================
class Storm {

    constructor() {
        this.rainDrops = [];
        this.targetRainDops = 50;
    }

    setRainAmount(amountOfRain) {
        var raindropCount = convertRange(0, 100, 0, canvas.width + canvas.height, amountOfRain);
        raindropCount = raindropCount | 0;
        this.targetRainDops = raindropCount;
        setStormSounds(amountOfRain);
    }

    makeRainDrop(offsetx = 0, offsety = 0) {
        this.rainDrops.push({
            x: Math.random() * (canvas.width + 200) - offsetx - 50,
            y: -20 - offsety,
            l: Math.random() * 1,
            xs: -2 + Math.random() * 2 + 1,
            ys: Math.random() * 10 + 8,
            endingY: Math.random() * (canvas.height + 200) - offsety + 20
        })
    }

    draw(c, offsetx, offsety) {
        c.strokeStyle = 'rgba(174,194,224,0.5)';
        c.lineWidth = 1;
        c.lineCap = 'round';
        for (var i = 0; i < this.rainDrops.length; i++) {
            var p = this.rainDrops[i];
            if (p.x <= 0 || p.y <= 0 || p.x >= currentMap.width || p.y >= currentMap.height) {
                continue;
            }
            c.beginPath();
            c.moveTo(p.x + offsetx, p.y + offsety);
            c.lineTo(p.x + offsetx + p.l * p.xs, p.y + offsety + p.l * p.ys);
            c.stroke();
        }
        this.move(offsetx, offsety);
    }

    move(offsetx, offsety) {
        var toremove = [];
        for (var b = 0; b < this.rainDrops.length; b++) {
            var p = this.rainDrops[b];
            p.x += p.xs;
            p.y += p.ys;
            if ((p.x + offsetx) > canvas.width || p.y + offsety > canvas.height || p.y > p.endingY) {
                if ((this.rainDrops.length - toremove.length) > this.targetRainDops) {
                    toremove.push(p);
                }
                p.x = Math.random() * (canvas.width + 200) - offsetx - 50;
                p.y = -20 - offsety;
                p.endingY = Math.random() * (canvas.height + 200) - offsety + 20
            }
        }
        for (var i = 0; i < toremove.length; i++) {
            var value = toremove[i];
            this.rainDrops = this.rainDrops.filter(function (item) {
                return item !== value;
            })
        }
        for (var a = this.rainDrops.length; a < this.targetRainDops; a++) {
            this.makeRainDrop(offsetx, offsety);
        }

    }

}