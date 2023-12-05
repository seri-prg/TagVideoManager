

// ボリューム保存セットアップ
function SetupKeepVolume(video)
{
    video.addEventListener("volumechange", (event) => 
    {
        localStorage.setItem('volume', event.target.volume);
    });
    
    // 以前のボリュームを復帰
    PullVolume(video);
}


// 以前のボリュームを復帰
function PullVolume(video)
{
    var volume = localStorage.getItem('volume');
    if(volume != null)
    {
        video.volume = volume;
    }
}



// プレイポイント削除管理
class TagRemover { 
    _target = null; // 削除ボタン
    _removeId = null; // 削除ID
    _opCode = null; // オペレーションコード

    onClick = null; // 削除ボタンクリック時

    setup(targetId) {
        this._target = document.getElementById(targetId);
        this._target.onclick = () => {
            this.onClick(this._opCode, this._removeId); 
        };
        this.hide();
    }

    // 削除情報設定
    setInfo(opCode, text, removeId)
    {
        this._target.textContent = text;
        this._removeId = removeId; // opCodeによって意味が変わる
        this._opCode = opCode; // r=タグリンクId ra=タグId
        this.#show();
    }
    

    #show() { this._target.style.display = "block"; }

    // ボタン非表示
    hide() { this._target.style.display = "none"; }
}




/*
    プレイポイントを追加するタグリスト
*/
class VideoTagList {
    _target = null; // 描画先
    _filterText = null; // 検索ボックスの文字

    // タグボタンを押した時のイベント
    onClickTag = null;

    setup(targetId, textBoxId) {
		this._target = document.getElementById(targetId);
        this.#setupTextBox(textBoxId);
    }

    // 検索ボックス登録
    #setupTextBox(textBoxId) {
		var tagText = document.getElementById(textBoxId);

		this._filterText = localStorage.getItem('tagFilterText');	// 別ページから引き継ぐ為
		if (this._filterText)
		{
			tagText.value = this._filterText;
			this.updateFilter();
		}

        // テキストボックス入力イベント
        tagText.oninput = (e)=> {
            this._filterText =  e.target.value;
			localStorage.setItem('tagFilterText', this._filterText);	// 別ページから引き継ぐ為
            this.updateFilter();
        }
    }


    // タグリスト更新
	update(jsonData)
	{
		RemoveChildAll(this._target);

		jsonData.forEach(element => 
		{
			const tag = document.createElement('button');
			tag.className = "tag_parts";
			tag.id = element.tag_id;
			tag.innerText = element.name;
            tag.onclick = (elem) => {
                this.onClickTag(elem.target);
            };
            this.#updateShowTag(tag);

            this._target.appendChild(tag);
		});
	}

    // タグ1つの表示更新
    #updateShowTag(elem) {
        elem.style.display = (elem.textContent.indexOf(this._filterText) >= 0)
                                                                ? "block" : "none";
    }


    // 任意の文字列を含むタグのみを表示
    updateFilter()
	{
		// フィルタリング 
		var tagList = document.querySelectorAll("tag_parts");
		tagList.forEach((i) => { this.#updateShowTag(i);});
	}
}


