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
		if (enableVrMode) {
			this._camera = new BABYLON.VRDeviceOrientationArcRotateCamera("DevOr_camera", -Math.PI / 2,  Math.PI / 2, 5, BABYLON.Vector3.Zero(), scene);
			//this._camera = new BABYLON.VRDeviceOrientationFreeCamera("Camera", new BABYLON.Vector3(-6.7, 1.2, -1.3), scene);
			// this._camera.setTarget(new BABYLON.Vector3(0, 0, 10));
		} else {
			this._camera = new BABYLON.ArcRotateCamera("Camera", -Math.PI / 2,  Math.PI / 2, 5, BABYLON.Vector3.Zero(), scene);
		}

		this._camera.attachControl(canvas, true);

		// this.addTest(scene);
		return scene;
	};


	addTest(scene) {
		 // This creates a light, aiming 0,1,0 - to the sky (non-mesh)
		 var light = new BABYLON.HemisphericLight("light", new BABYLON.Vector3(0, 1, 0), scene);
	
		 //Materials
		 var redMat = new BABYLON.StandardMaterial("red", scene);
		 redMat.diffuseColor = new BABYLON.Color3(1, 0, 0);
		 redMat.emissiveColor = new BABYLON.Color3(1, 0, 0);
		 redMat.specularColor = new BABYLON.Color3(1, 0, 0);
		 
		 var greenMat = new BABYLON.StandardMaterial("green", scene);
		 greenMat.diffuseColor = new BABYLON.Color3(0, 1, 0);
		 greenMat.emissiveColor = new BABYLON.Color3(0, 1, 0);
		 greenMat.specularColor = new BABYLON.Color3(0, 1, 0);
		 
		 var blueMat = new BABYLON.StandardMaterial("blue", scene);
		 blueMat.diffuseColor = new BABYLON.Color3(0, 0, 1);
		 blueMat.emissiveColor = new BABYLON.Color3(0, 0, 1);
		 blueMat.specularColor = new BABYLON.Color3(0, 0, 1);
		 
		 // Shapes
		 var plane1 = new BABYLON.Mesh.CreatePlane("plane1", 3, scene, true, BABYLON.Mesh.DOUBLESIDE);
		 plane1.position.x = -3;
		 plane1.position.z = 0;
		 plane1.material = redMat;
		 
		 var plane2 = new BABYLON.Mesh.CreatePlane("plane2", 3, scene, true, BABYLON.Mesh.DOUBLESIDE);
		 plane2.position.x = 3;
		 plane2.position.z = -1.5;
		 plane2.material = greenMat;
		 
		 var plane3 = new BABYLON.Mesh.CreatePlane("plane3", 3, scene, true, BABYLON.Mesh.DOUBLESIDE);
		 plane3.position.x = 3;
		 plane3.position.z = 1.5;
		 plane3.material = blueMat;
		 
		 var ground = BABYLON.Mesh.CreateGround("ground1", 10, 10, 2, scene);
	}



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
