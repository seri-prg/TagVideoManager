
// 動画を変換しながらストリーム再生
class VideoStream { 

    #mediaSource = new MediaSource();
    #sourceBuffer = null;
    #fileId = "";
    #startSecond = 0;	// 開始時間
    #timeSpanSecond = 3; // 再生時間
    #video = null;


    // 動画パスを取得
    #GetVideoPath() {
        return this.#fileId + "-" + this.#startSecond + "-" + this.#timeSpanSecond + ".test_video";
    }

    // 動画の時間更新
    #UpdateTime() {
        this.#startSecond += this.#timeSpanSecond;
    }
    
    
    
    Setup(fileId, videoName) {
        if (!window.MediaSource) {
            console.log('The Media Source Extensions API is not supported.');
            return;
        }
        this.#fileId = fileId;
        this.#video = document.getElementById(videoName);
        this.#video.src = URL.createObjectURL(this.#mediaSource);
        this.#mediaSource.addEventListener('sourceopen', this.#sourceOpen.bind(this));
    }

    
    #sourceOpen(e) {
    //	URL.revokeObjectURL(vidElement.src);
        //var mime = 'video/webm; codecs="vp9, vorbis"';
        // var mime = 'video/webm; codecs="vp8, vorbis"';
        var mime = 'video/webm; codecs="vp8"';
        if (MediaSource.isTypeSupported(mime)){
            console.log("supported ! " + mime);
        }

        this.#sourceBuffer = this.#mediaSource.addSourceBuffer(mime);
        this.#sourceBuffer.mode = 'sequence';

        this.#sourceBuffer.addEventListener("error", (e)=> {
            console.log("error !!");
            // console.dir(e);
            //console.error("error:", e);
        });


        fetch(this.#GetVideoPath())
            .then((response) => { return response.arrayBuffer(); })
            .then((arrayBuffer) => {
                this.#UpdateTime();

                // 更新時データ登録
                this.#sourceBuffer.addEventListener('updateend', (e) => {

                    this.#video.play();
                    console.log("update : [" + this.#sourceBuffer.updating + "] state:" + this.#mediaSource.readyState);
                    var path = this.#GetVideoPath();
                    this.#UpdateTime();
                    fetch(path)
                        .then((response) => { return response.arrayBuffer();})
                        .then((arrayBuffer) => {
                            if ((this.#mediaSource.readyState === 'open') && (arrayBuffer.byteLength <= 0)) {
                                this.#mediaSource.endOfStream();
                                return;
                            }
                            this.#sourceBuffer.appendBuffer(arrayBuffer); 
                        });
                });

                this.#sourceBuffer.appendBuffer(arrayBuffer);
            });
    }
}



