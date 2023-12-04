/*
	Babylonjsを利用したVR動画再生
*/

class BBRender {
	engine = null;
	scene = null;
	sceneToRender = null;
	videoDome = null;

	static createScene (canvas) {
		var scene = new BABYLON.Scene(this.engine);
		var camera = new BABYLON.ArcRotateCamera("Camera", -Math.PI / 2,  Math.PI / 2, 5, BABYLON.Vector3.Zero(), scene);
		camera.attachControl(canvas, true);
		return scene;
	};

	static createDefaultEngine (canvas) {
		return new BABYLON.Engine(canvas, true, 
		{
			preserveDrawingBuffer: true, stencil: true,  disableWebGL2Support: false
		});
	};

	static async asyncEngineCreation(canvas) {
		try {
			return BBRender.createDefaultEngine(canvas);
		} catch(e) {
			console.log("the available createEngine function failed. Creating the default engine instead");
			return BBRender.createDefaultEngine(canvas);
		}
	}

	renderLoop() {
		if ((this.sceneToRender != null) && this.sceneToRender.activeCamera) {
			this.sceneToRender.render();
		}
	}

	async initFunction(canvas) {
		this.engine = await BBRender.asyncEngineCreation(canvas);
		if (!this.engine) throw 'engine should not be null.';
		this.engine.runRenderLoop(this.renderLoop.bind(this));
		this.scene = BBRender.createScene(canvas);
	};

	play(url)
	{
		if (this.videoDome != null)
		{
			this.videoDome.dispose(false, true);
		}

		this.videoDome = new BABYLON.VideoDome(
			"videoDome",
			url,
			{
				resolution: 32,
				// fovMultiplierを使う場合はこちらをfalseにする必要があるが、
				// falseにすると動的にテクスチャソースの分割が変わらない？
				useDirectMapping: true,
				size: 1000,
				clickToPlay: false,
				// autoPlay : true,
			},
			this.scene
		);
	}

	getVideoElem() {
		if (this.videoDome == null)
			return null;

		return this.videoDome.texture.video;
	}


	// 180 or 360 dome
	setHalf(half) {
		if (this.videoDome == null)
			return;
		this.videoDome.halfDome = half;
	}

	// zoom(0.0 - 2.0: default 1.0)
	setFovMultiplier(value) {
		if (this.videoDome == null)
			return;
		this.videoDome.fovMultiplier = value;
	}


	/*
	BABYLON.VideoDome.MODE_MONOSCOPIC : Define the texture source as a Monoscopic panoramic 360.
	BABYLON.VideoDome.MODE_TOPBOTTOM : Define the texture source as a Stereoscopic TopBottom/OverUnder panoramic 360.
	BABYLON.VideoDome.MODE_SIDEBYSIDE : Define the texture source as a Stereoscopic Side by Side panoramic 360.
	*/
	setVideoMode(mode) {
		if (this.videoDome == null)
			return;
		this.videoDome.videoMode = mode;
	}

	/*
		"MODE_MONOSCOPIC" : Define the texture source as a Monoscopic panoramic 360.
		"MODE_TOPBOTTOM" : Define the texture source as a Stereoscopic TopBottom/OverUnder panoramic 360.
		"MODE_SIDEBYSIDE" : Define the texture source as a Stereoscopic Side by Side panoramic 360.
	*/
	setVideoModeFromString(modeStr) {
		var mode = BABYLON.VideoDome.MODE_MONOSCOPIC;
		if (modeStr == "MODE_SIDEBYSIDE")
		{
			mode = BABYLON.VideoDome.MODE_SIDEBYSIDE;
		}
		else if (modeStr == "MODE_TOPBOTTOM")
		{
			mode = BABYLON.VideoDome.MODE_TOPBOTTOM;
		}
		// console.log(mode);
		this.setVideoMode(mode);
	}


	setup(canvasName, onReadyCallback = null)
	{
		var canvas = document.getElementById(canvasName);
		this.initFunction(canvas).then(() => {
			this.sceneToRender = this.scene

			// Resize
			window.addEventListener("resize", function () {
				this.engine.resize();
			}.bind(this));

			if (onReadyCallback != null)
			{
				onReadyCallback(this);
			}

			this.engine.resize();
		});

	}
}
