/*
	jsonから情報を取得してサムネイルを表示
	サムネイルをクリックするとそこから動画再生

*/
class PlayPoint {

	targetElem = null; // プレイポイントを実行する場所
	lastSelectPoint = null; // 最後に選択したポイント
	videoElem = null; // 再生対象

	// イベント
	onSelect = null; // プレイポイント選択時
	onChangeTagNode = null; // タグ開閉時


	// ターゲットの位置
	setTarget(elemName, videoName)
	{
		this.targetElem = document.getElementById(elemName);
		this.videoElem = document.getElementById(videoName);
	}

	// 操作対象の動画取得
	setControlVideo(videoElem)
	{
		this.videoElem = videoElem;	
	}


	// プレイポイントの更新リクエスト
	update(json)
	{
		RemoveChildAll(this.targetElem);
		this.create(json); // プレイポイント更新
	}



	// プレイポイント選択時
	#createPlayPoint(element, t)
	{
		const play_btn = document.createElement('img');
		play_btn.className = "play_btn";
		play_btn.id = t.id;
		play_btn.src="./"+ t.link_img;
		play_btn.setAttribute("tag_name", element.name);
		play_btn.setAttribute("play_time", t.second);
		play_btn.setAttribute("play_disp", t.disp);

		play_btn.onclick = (e)=>
		{
			var elem = e.target;
			// 選択画像のフレームを目立たせる
			if (this.lastSelectPoint != null) {
				this.lastSelectPoint.setAttribute("border", 0);
			}
			//console.log("elem.id:"+ elem.id) ;
			elem.setAttribute("border", "5px solid #f00");

			// 動画再生
			var time = elem.getAttribute("play_time");
			if (time == null) {
				time = 0;
			}
			this.videoElem.currentTime = time;

			this.lastSelectPoint = elem;

			if (this.onSelect != null) {
				this.onSelect(elem, time)
			}
		};

		return play_btn;
	}

	// タグの開閉
	#ToggleTagNode(target)
	{
        target.parentElement.querySelector(".nested").classList.toggle("active");
        target.classList.toggle("caret-down");
	}


	create(jsonData)
	{
		const tagTimeBase = document.createElement("ul");
		tagTimeBase.id = "tag_time_base";

		// tag毎の処理
		jsonData.tag_list.forEach(element => 
		{
			const group = document.createElement('li');
			group.style.backgroundColor = element.color;

			// タグ１つ
			var caret = document.createElement('span');
			caret.className = "caret";
			caret.id = element.id;
			caret.innerText = element.name;
			caret.onclick = (elem) => {
				this.#ToggleTagNode(elem.target);
				if (this.onChangeTagNode != null)
				{
					this.onChangeTagNode(elem.target);	// タグ変更時
				}
			}

			// caret.style = "border: solid 2px #ff0000;";
			const nested = document.createElement('ul');
			nested.className = "nested";
			nested.setAttribute("tag_name", element.name);


			// 時間ごとの処理
			element.time_list.forEach(t => 
			{
				if (t.second <= 0.0001)
					return;

				const play_rect = document.createElement('div');
				play_rect.className = "play_rect";

				const play_btn = this.#createPlayPoint(element, t);
				
				const play_rect_time = document.createElement('div');
				play_rect_time.className = "play_rect_time";
				play_rect_time.innerText = t.disp;

				play_rect.appendChild(play_btn);
				play_rect.appendChild(play_rect_time);
				nested.appendChild(play_rect);
				nested.appendChild(document.createElement("br"));
			});

			group.appendChild(caret);
			group.appendChild(nested);

			this.#ToggleTagNode(caret);	// tagを開く

			tagTimeBase.appendChild(group);
		});

		this.targetElem.appendChild(tagTimeBase);
	}
}
