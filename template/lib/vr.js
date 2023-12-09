/*
	Babylonjsを利用したVR動画再生
*/

class BBRender {
	engine = null;
	scene = null;
	sceneToRender = null;
	videoDome = null;
	_camera = null;

	async #createScene (canvas, enableVrMode) {
		var scene = new BABYLON.Scene(this.engine);
		// 注視点固定カメラ
		this._camera = new BABYLON.ArcRotateCamera("Camera", -Math.PI / 2,  Math.PI / 2, 5, BABYLON.Vector3.Zero(), scene);
		this._camera.attachControl(canvas, true);

		try {
			// vrモードが有効なら
			if (enableVrMode) {
				var vrHelper = scene.createDefaultVRExperience({createDeviceOrientationCamera:true, useXR: true});
				// need https server
				// var vrHelper = await scene.createDefaultXRExperienceAsync({});
			}
		} catch (e) {

		}

		return scene;
	};


	static createDefaultEngine (canvas) {
		return new BABYLON.Engine(canvas, true, {
			preserveDrawingBuffer: true, 
			stencil: true,  
			disableWebGL2Support: false
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

	async initFunction(canvas, enableVrMode) {
		this.engine = await BBRender.asyncEngineCreation(canvas);
		if (!this.engine) throw 'engine should not be null.';
		this.engine.runRenderLoop(this.renderLoop.bind(this));
		this.scene = await this.#createScene(canvas, enableVrMode);
	};

	play(url)
	{
		if (this.videoDome != null) {
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

		if (this._camera != null) {
			if (half) {
				this._camera.setPosition(new BABYLON.Vector3(0, 0, -1));
			} else {
				this._camera.setPosition(new BABYLON.Vector3(1, 0, 0));
			}
		}
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
		if (modeStr == "MODE_SIDEBYSIDE") {
			mode = BABYLON.VideoDome.MODE_SIDEBYSIDE;
		} else if (modeStr == "MODE_TOPBOTTOM") {
			mode = BABYLON.VideoDome.MODE_TOPBOTTOM;
		}
		// console.log(mode);
		this.setVideoMode(mode);
	}


	setup(canvasName, enableVrMode, onReadyCallback = null)
	{
		var canvas = document.getElementById(canvasName);
		this.initFunction(canvas, enableVrMode).then(() => {
			this.sceneToRender = this.scene

			if (onReadyCallback != null) {
				onReadyCallback(this);
			}

			this.engine.resize();

			// Resize
			window.addEventListener("resize", () => {
				this.engine.resize();
			});

		});

	}
}
