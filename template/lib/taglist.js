/*
	タグのリスト（チェックボックス)とその上に検索ボックスのをコントロールする
	・検索ボックスの内容をで前方一致でフィルタリング
	・タグのドラッグ＆ドロップ
	・タグのチェックボックスon/off時の処理
	

使用例(html)
<input type="text" id="tag_text" ><button id="tag_add">追加</button><br>
<div class="tag_parts" id="tag1"><input type="checkbox" class="chb" id="tag1">お気に入り</div>
<div class="tag_parts" id="tag2"><input type="checkbox" class="chb" id="tag2">TAG_NAME</div>
<div class="tag_parts" id="tag3"><input type="checkbox" class="chb" id="tag3">Sample</div>
<div class="tag_parts" id="tag4"><input type="checkbox" class="chb" id="tag4">TAG_ID</div>



使用例(呼び出し)
	document.addEventListener('DOMContentLoaded', function() 
	{
		// D&D登録(ドラック元class名, ドロップ先クラス名)
		setupDrop(".tag_parts", ".video_item", (sendElem, receiveElem) =>
		{
			document.getElementById('state').innerHTML = sendElem.id + " => " + receiveElem.id + "!!";
		});

		// タグのフィルタリング登録(検索用テキストボックスid、検索対象クラス名)
		setupTagFilter("tag_text", ".tag_parts");

		// タグの追加ボタン(登録用テキストボックスid、送信ボタンid)
		setupTagAdd("tag_text", "tag_add", (msg)=>
		{
			document.getElementById('state').innerHTML = "add tag : " + msg;
		});

		// タグのチェック変更時(検索対象チェックボックスクラス名)
		setupChangeTag(".chb", (msg)=>
		{
			// デバッグ表示
			document.getElementById('state').innerHTML = msg + "!!!";
		});

	}, false);
*/


	// タグの追加ボタン
	function setupTagAdd(tagText, tagAdd, addCallback)
	{
		var elem = document.getElementById(tagAdd);
		elem.addEventListener("click", (event) =>
		{
			var elemText = document.getElementById(tagText);
			addCallback(elemText.value);
		});
	}


	// 検索ボックスの内容を見てタグリストの表示を更新
	function updateTagFilterString(enableString, tagPartsClass)
	{
		// フィルタリング 
		var tagList = document.querySelectorAll(tagPartsClass);
		tagList.forEach((i) => {
			if (i.textContent.indexOf(enableString) >= 0) {
				i.style.display = "block";
			}
			else {
				i.style.display = "none";
			}
		});
	}
	

	// 検索ボックスの内容を見てタグリストの表示を更新
	function updateTagFilter(textBox, tagPartsClass)
	{
		// 検索ボックスの文字
		var tagText = document.getElementById(textBox);
		updateTagFilterString(tagText.value, tagPartsClass);
	}

	// 検索ボックス(+タグ追加)変更時
	function setupTagFilter(textBox, tagPartsClass)
	{
		var tagText = document.getElementById(textBox);
		var ftext = localStorage.getItem('tagFilterText');	// 別ページから引き継ぐ為
		if (ftext)
		{
			tagText.value = ftext;
			updateTagFilterString(tagText.value, tagPartsClass);
		}

		tagText.addEventListener("input", (event) =>
		{
			// 検索ボックスの内容を見てタグリストの表示を更新
			updateTagFilterString(event.target.value, tagPartsClass);
			localStorage.setItem('tagFilterText', event.target.value);	// 別ページから引き継ぐ為
		});
	}


	// チェックの入っているチェックボックスのリストを取得
	function getCheckedIdList(className)
	{
		var chbList = document.querySelectorAll(className);
		return Array.from(chbList)
					.filter((c) => c.checked)
					.map((c) => c.id);
	}

	// チェックの入っている情報をクエリパラメータで取得
	function getCheckedIdQuery(className)
	{
		return getCheckedIdList(className).join(',');
	}

	// チェック状態を設定
	function setCheckedId(className, checkList)
	{
		var tagList = document.querySelectorAll(className);
		tagList.forEach((i) => 
		{
			i.checked = checkList.includes(i.id);
		});
	}


	// タグチェック変更時
	function setupChangeTag(tagCheckClass, changeCallback)
	{
		var tagList = document.querySelectorAll(tagCheckClass);
		tagList.forEach((i) => 
		{
			// イベントの重複登録を防ぐ
			if (i.getAttribute("change_tag")!=null)
				return;
		
			i.setAttribute("change_tag", "true");

			i.addEventListener("click", (evnet)=>
			{
				// 変更時呼び出し処理
				changeCallback();
			});
		});
	}



	// ドラッグ＆ドロップイベントテスト
	var dragged = null;
	function setupDrop(sendClass, receiveClass, sendCallback)
	{
		// 送り側
		setupSendDrag(sendClass);
	
		// 受け取り側
		setupReceiveDrop(receiveClass, sendCallback);
	}

	// ドラッグ＆ドロップ送信時処理
	function setupSendDrag(sendClass)
	{
		var dragList = document.querySelectorAll(sendClass);
		dragList.forEach((i) => 
		{
			// イベントの重複登録を防ぐ
			if (i.getAttribute("send_tag_drag")!=null)
				return;

			i.setAttribute("send_tag_drag", "true");

			i.draggable='true';
			// ドラッグ開始
			i.addEventListener("dragstart", (event) => {
				// ドラッグ中の要素の参照を保存
				dragged = event.target;
			});
		});
	}
	

	// ドラッグ＆ドロップ受け取り側イベント追加
	function setupReceiveDrop(receiveClass, sendCallback)
	{
		var dropList = document.querySelectorAll(receiveClass);
		dropList.forEach((i) => 
		{
			// イベントの重複登録を防ぐ
			if (i.getAttribute("tag_drop")!=null)
				return;

			i.setAttribute("tag_drop", "true");
			// ドラッグ中
			i.addEventListener("dragover", (event)=> {
				event.preventDefault();
			});

			// ドロップ時
			i.addEventListener("drop", (event)=> {
				event.preventDefault();

				// ブラウザ外からのファイルドロップを制限
				if (event.dataTransfer.files.length > 0) {
					return;
				}
				sendCallback(dragged, i);
				dragged = null;
			});
		});

	}



	// 矩形が表示されているか監視
	function setupViewLook(target, callback)
	{
		var targetElem = document.getElementById(target);
		if (targetElem === null)
			return;

		// イベントの重複登録を防ぐ
		if (targetElem.getAttribute("view_look_event")!=null)
			return;

		targetElem.setAttribute("view_look_event", "true");

		const intersectptions = {
			root : null,
			rootMargin : '0px',
			threshold : 0
		};

		const observer = new IntersectionObserver((entries)=>
		{
			entries.forEach((entry) => {
				if (entry.isIntersecting) {
					callback(entry);
				}
			});
		}, intersectptions);
		observer.observe(targetElem);
	}


	// 画像データを２つ上の親エレメントの矩形に収まるようにサイズ調整
	// 縦横比は維持する。
	function AdjustImg(targetImg)
	{
		const w = targetImg.naturalWidth;
		const h = targetImg.naturalHeight;

		// wかhが0なら無処理
		if ((w * h) == 0)
			return;

		const wrate = w/h;
		const par = targetImg.parentElement.parentElement;

		// 画像の縦横比が表示矩形より横に長い場合
		if (wrate > (par.clientWidth/par.clientWidth))
		{
			targetImg.width = par.clientWidth;
			targetImg.height = par.clientWidth * h / w;
		}
		// それ以外は縦に長い
		else
		{
			// 高さをマックスにして幅は比率に合わせる
			targetImg.width = par.clientHeight * w / h;
			targetImg.height = par.clientHeight;
		}
	}

	var firstAdjustResize = false;
	// 画像サイズ自動調整
	function SetupImageAdjust(imageClass)
	{
		// 画像のロードが終わった場合

		var images = document.querySelectorAll(imageClass);
		images.forEach((i) => 
		{
			// 既にロードが終わってる場合の対応
			AdjustImg(i);

			if (i.getAttribute("image_adjust")!=null)
				return;

			i.setAttribute("image_adjust", "true");

			i.addEventListener('load', (elem) => {
				AdjustImg(elem.target);

			}, false);
		});
/*
		// 初めてこの関数が呼ばれたときだけリサイズイベント登録
		if (firstAdjustResize == false)
		{
			firstAdjustResize = true;

			// windowサイズが変更した場合
			window.addEventListener('resize', (event)=>
			{
				var images = document.querySelectorAll(imageClass);
				images.forEach((i) => 
				{
					AdjustImg(i);
				});
			});
		}
		*/
	}