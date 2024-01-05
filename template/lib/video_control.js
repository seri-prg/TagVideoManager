




// 自前動画操作
class CustomVideoControl {
    _video = null;
    _timeElem = null; // 再生時間表示
    _progressElem = null; // シークバー
    _progressMoving = false; // シークバー操作中
    _videoPlayedState = false; // シークバーが操作される前に動画が再生状態だったか
    _volumeElem = null;
    _muteElem = null;
    _isMute = false; // ミュート中か否か
    _lastMuteVolume = 0; // ミュートが押された時のボリューム
    _speedBarElem = null; 
    _cameraFov = null;
    _editor = null; // コントロール全体

    _bbRender = null; // 3Dレンダラ
    _iconPlay = null;
    _iconSound = null;


    // 秒を00:00:00の表記に変換
    static getDispTime(time)
    {
        const totalSec = Math.floor(time);
        const sec = totalSec % 60; // 秒
        const min = Math.floor(totalSec / 60) % 60; // 分
        const hour = Math.floor(totalSec / 3600); // 分

        return hour.toString().padStart( 2, '0') + ":" + 
                min.toString().padStart( 2, '0') + ":" + 
                sec.toString().padStart( 2, '0');
    }



    // ポーズなら再生アイコン、再生中なら停止アイコンを表示
    #showPlayIcon(isPause) {
        this._iconPlay.src = (isPause) ? "./icon/icon_play.png" : "./icon/icon_pause.png";
    }


    // ビデオの状態から表示を更新
    updateVideoInfo()
    {
        if (this._video != null) {
            this._volumeElem.value = this._video.volume * 100;	// ボリューム設定
            this.#showPlayIcon(this._video.paused);
        }
    }

    // コントロール非表示
    hideControl()
    {
        this._editor.style.opacity  = 0.0;
    }


    setup(setObj)
    {
        this._video = setObj.videoElem;
        
        this._timeElem = document.getElementById(setObj.timeIdName);
        this._progressElem = document.getElementById(setObj.progressIdName);
        this._volumeElem = document.getElementById(setObj.volumeIdName);
        this._muteElem = document.getElementById(setObj.muteIdName);
        this._speedBarElem= document.getElementById(setObj.speedBarIdName); // スピードバー
        this._iconPlay = document.createElement('img'); // 再生アイコン
        this._iconSound = document.createElement('img'); // サウンドアイコン
        this._bbRender = setObj.bbRender;

        this._muteElem.appendChild(this._iconSound);


        // コントローラーの表示on/off
        this._editor = document.getElementById(setObj.editorIdName);
        this._editor.onmouseenter = (elem) => {
            elem.target.style.opacity  = 1;
        }
        this._editor.onmouseleave = (elem) => {
            // 非表示にするのは再生中の時だけ
            if (!this._video.paused) {
                this.hideControl();
            }
        }


        // カメラの距離スライダー
        this._cameraFov = document.getElementById(setObj.cameraBarIdName);
        this._cameraFov.oninput = (elem) => {
            console.log("camera : " + elem.target.value * 0.01);
            this._bbRender.setFovMultiplier(elem.target.value * 0.01);
        }
        // カメラの距離をデフォルトに戻す
        document.getElementById(setObj.cameraResetIdName).onclick = ()=> {
            this._bbRender.setFovMultiplier(1.0);
            this._cameraFov.value = 100;
        };


        // 再生速度リセット
        document.getElementById(setObj.speedResetIdName).onclick = (elem) => {
            this._video.playbackRate = 1.0;
            this._speedBarElem.value = this._video.playbackRate * 100;
        }

        // ビデオの状態から表示を更新
        this.updateVideoInfo();

        var playBtn = document.getElementById(setObj.play);
        playBtn.appendChild(this._iconPlay);
        // 再生のon/off
        playBtn.onclick = (elem) => {
            var target = elem.target;
            if (this._video.paused)	{
                this._video.play();
            } else {
                this._video.pause();
            }
        };

        this._video.onplay = () =>{
            this.#showPlayIcon(false);
        }

        this._video.onpause = () =>{
            this.#showPlayIcon(true);
        }

        // 動画の再生時間変更
        this._video.ontimeupdate = ()=>{
            this._timeElem.textContent
                            = CustomVideoControl.getDispTime(this._video.currentTime) + " / " +
                            CustomVideoControl.getDispTime(this._video.duration);

            // シークバーを動かしていない場合は設定
            if (!this._progressMoving) {
                this._progressElem.max = this._video.duration * 10;	// 0.1秒単位
                this._progressElem.value = this._video.currentTime * 10;
            }
        }

        this._video.onvolumechange = ()=>{
            // 音量によってアイコン表示を変更
            if (this._video.volume > 0.0) {
                this._iconSound.src = "./icon/icon_sound.png";
            } else {
                this._iconSound.src = "./icon/icon_sound_mute.png";
            }
        }


        // スピードバー変更時
        this._speedBarElem.oninput = ()=>{
            this._video.playbackRate = this._speedBarElem.value * 0.01;
        }

        // シークバー移動中
        this._progressElem.oninput = () => {
            // シークバー動かし始め奈良
            if (!this._progressMoving) {
                this._videoPlayedState = !this._video.paused; // 直前の再生状態を保存
                this._video.pause();
                this._progressMoving = true;
            }
            this._video.currentTime = this._progressElem.value * 0.1;
        }
        // シークバー移動終了時
        this._progressElem.onchange = () => {
            this._progressMoving = false;
            if (this._videoPlayedState)	{
                this._video.play();
            }
        }

        // ミュートボタン
        this._muteElem.onclick = () => {
            // ミュートだった
            if (this._isMute) {
                this._video.volume = this._lastMuteVolum; // ボリューム復帰
            // ミュートでなかった
            } else {
                this._lastMuteVolum = this._video.volume; // ボリュームの保存
                this._video.volume = 0.0;
            }

            this._volumeElem.value = this._video.volume * 100;	// ボリュームバーを更新
            this._isMute = !this._isMute; // ミュート状態を切り替え
        }


        // ボリューム
        this._volumeElem.oninput = () => {
            this._video.volume = this._volumeElem.value * 0.01;

            // ボリュームが更新されたならミュート状態会場
            if (this._video.volume > 0.0) {
                this._isMute = false;
            }
        };
    }
}




