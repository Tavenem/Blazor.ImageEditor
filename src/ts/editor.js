class EllipseDrawTool {
    constructor() {
        this.originX = 0;
        this.originY = 0;
        this.drawingMode = 0;
    }
    make(startX, startY, endX, endY, options, shift) {
        this.originX = startX;
        this.originY = startY;
        if (shift) {
            const size = Math.max(Math.abs(startX - endX), Math.abs(startY - endY));
            endX = startX > endX
                ? startX - size
                : startX + size;
            endY = startY > endY
                ? startY - size
                : startY + size;
        }
        return new fabric.Ellipse({
            left: endX < startX ? endX : startX,
            top: endY < startY ? endY : startY,
            rx: Math.abs(endX - startX) / 2,
            ry: Math.abs(endX - startX) / 2,
            ...options
        });
    }
    resize(object, x, y, ctl, alt, shift) {
        if (object instanceof fabric.Ellipse) {
            if (shift) {
                const size = Math.max(Math.abs(this.originX - x), Math.abs(this.originY - y));
                x = this.originX > x
                    ? this.originX - size
                    : this.originX + size;
                y = this.originY > y
                    ? this.originY - size
                    : this.originY + size;
            }
            object.set({
                originX: this.originX > x ? 'right' : 'left',
                originY: this.originY > y ? 'bottom' : 'top',
                rx: Math.abs(this.originX - x) / 2,
                ry: Math.abs(this.originY - y) / 2,
            }).setCoords();
        }
        return object;
    }
}
class LineDrawTool {
    constructor() {
        this.drawingMode = 1;
    }
    make(startX, startY, endX, endY, options, shift) {
        if (shift) {
            const sizeX = Math.abs(startX - endX);
            const sizeY = Math.abs(startY - endY);
            const maxSize = Math.max(sizeX, sizeY);
            if (Math.abs(sizeX - sizeY) <= 3) {
                endX = startX > endX
                    ? startX - maxSize
                    : startX + maxSize;
                endY = startY > endY
                    ? startY - maxSize
                    : startY + maxSize;
            }
            else if (sizeX > sizeY) {
                endX = startX > endX
                    ? startX - maxSize
                    : startX + maxSize;
                endY = startY;
            }
            else {
                endX = startX;
                endY = startY > endY
                    ? startY - maxSize
                    : startY + maxSize;
            }
        }
        return new fabric.Line([startX, startY, endX, endY], options);
    }
    resize(object, x, y, ctl, alt, shift) {
        if (object instanceof fabric.Line) {
            if (shift) {
                const sizeX = Math.abs((object.x1 ?? 0) - x);
                const sizeY = Math.abs((object.y1 ?? 0) - y);
                const maxSize = Math.max(sizeX, sizeY);
                if (Math.abs(sizeX - sizeY) <= 3) {
                    x = (object.x1 ?? 0) > x
                        ? (object.x1 ?? 0) - maxSize
                        : (object.x1 ?? 0) + maxSize;
                    y = (object.y1 ?? 0) > y
                        ? (object.y1 ?? 0) - maxSize
                        : (object.y1 ?? 0) + maxSize;
                }
                else if (sizeX > sizeY) {
                    x = (object.x1 ?? 0) > x
                        ? (object.x1 ?? 0) - maxSize
                        : (object.x1 ?? 0) + maxSize;
                    y = (object.y1 ?? 0);
                }
                else {
                    x = (object.x1 ?? 0);
                    y = (object.y1 ?? 0) > y
                        ? (object.y1 ?? 0) - maxSize
                        : (object.y1 ?? 0) + maxSize;
                }
            }
            object.set({
                x2: x,
                y2: y
            }).setCoords();
        }
        return object;
    }
}
class PathDrawTool {
    constructor() {
        this.drawingMode = 1;
    }
    addPoint(object, x, y) {
        const path = object.path;
        const points = path.length + 1;
        path.push([
            'Q',
            x, y,
            x, y
        ]);
        const center = object.getCenterPoint(), newPathObject = new fabric.Path(path);
        object.set({
            path: newPathObject.path,
            width: newPathObject.width,
            height: newPathObject.height,
            dirty: true,
            pathOffset: newPathObject.pathOffset
        }).setCoords();
        object.setPositionByOrigin(center, 'center', 'center');
        this.addControlPoint(object, points - 1);
    }
    make(startX, startY, endX, endY, options, shift) {
        const path = [[], []];
        if (shift) {
            const sizeX = Math.abs(startX - endX);
            const sizeY = Math.abs(startY - endY);
            const maxSize = Math.max(sizeX, sizeY);
            if (Math.abs(sizeX - sizeY) <= 3) {
                endX = startX > endX
                    ? startX - maxSize
                    : startX + maxSize;
                endY = startY > endY
                    ? startY - maxSize
                    : startY + maxSize;
            }
            else if (sizeX > sizeY) {
                endX = startX > endX
                    ? startX - maxSize
                    : startX + maxSize;
                endY = startY;
            }
            else {
                endX = startX;
                endY = startY > endY
                    ? startY - maxSize
                    : startY + maxSize;
            }
        }
        path[0][0] = 'M';
        path[0][1] = startX;
        path[0][2] = startY;
        path[1][0] = 'Q';
        path[1][1] = endX;
        path[1][2] = endY;
        path[1][3] = endX;
        path[1][4] = endY;
        const obj = new fabric.Path(path, options);
        obj.controls = { ...fabric.Object.prototype.controls };
        obj.controls['csc'] = new fabric.Control({
            positionHandler: this.curvePositionHandler,
            actionHandler: this.curveActionHandler,
            actionName: 'modifyCurve',
            pointIndex: 0
        });
        this.addControlPoint(obj, 1);
        return obj;
    }
    resize(object, x, y, ctl, alt, shift) {
        if (object instanceof fabric.Path && object.path) {
            const objectPath = object.path;
            const movingPointIndex = objectPath.length - 1;
            if (shift && movingPointIndex === 1) {
                const sizeX = Math.abs(objectPath[0][1] - x);
                const sizeY = Math.abs(objectPath[0][2] - y);
                const maxSize = Math.max(sizeX, sizeY);
                if (Math.abs(sizeX - sizeY) <= 3) {
                    x = objectPath[0][1] > x
                        ? objectPath[0][1] - maxSize
                        : objectPath[0][1] + maxSize;
                    y = objectPath[0][2] > y
                        ? objectPath[0][2] - maxSize
                        : objectPath[0][2] + maxSize;
                }
                else if (sizeX > sizeY) {
                    x = objectPath[0][1] > x
                        ? objectPath[0][1] - maxSize
                        : objectPath[0][1] + maxSize;
                    y = objectPath[0][2];
                }
                else {
                    x = objectPath[0][1];
                    y = objectPath[0][2] > y
                        ? objectPath[0][2] - maxSize
                        : objectPath[0][2] + maxSize;
                }
            }
            const path = objectPath;
            path[movingPointIndex][1] = x;
            path[movingPointIndex][2] = y;
            path[movingPointIndex][3] = x;
            path[movingPointIndex][4] = y;
            const center = object.getCenterPoint(), newPathObject = new fabric.Path(path);
            object.set({
                path: newPathObject.path,
                width: newPathObject.width,
                height: newPathObject.height,
                dirty: true,
                pathOffset: newPathObject.pathOffset
            });
            if (movingPointIndex === 1) {
                object.set({
                    left: x < objectPath[0][1]
                        ? x
                        : objectPath[0][1],
                    top: y < objectPath[0][2]
                        ? y
                        : objectPath[0][2]
                });
            }
            object.setCoords();
            if (movingPointIndex !== 1) {
                object.setPositionByOrigin(center, 'center', 'center');
            }
        }
        return object;
    }
    addControlPoint(object, index) {
        object.controls[`cqc${index}`] = new fabric.Control({
            positionHandler: this.curvePositionHandler,
            actionHandler: this.curveActionHandler,
            actionName: 'modifyCurve',
            pointIndex: index,
            pointIsCurve: true,
        });
        object.controls[`cpc${index}`] = new fabric.Control({
            positionHandler: this.curvePositionHandler,
            actionHandler: this.curveActionHandler,
            actionName: 'modifyCurve',
            pointIndex: index
        });
    }
    curveActionHandler(eventData, transform, x, y) {
        if (!(transform.target instanceof fabric.Path)) {
            return false;
        }
        const activeItem = transform.target, path = activeItem.path;
        const currentControl = activeItem.controls[activeItem.__corner], baseSize = activeItem._getNonTransformedDimensions(), size = activeItem._getTransformedDimensions(), strokeWidth = activeItem.strokeWidth ?? 1, stroke = {
            x: activeItem.strokeUniform ? strokeWidth : strokeWidth * (activeItem.scaleX ?? 1),
            y: activeItem.strokeUniform ? strokeWidth : strokeWidth * (activeItem.scaleY ?? 1)
        }, scaleX = (baseSize.x - strokeWidth) / (size.x - stroke.x), scaleY = (baseSize.y - strokeWidth) / (size.y - stroke.x), localPoint = activeItem.toLocalPoint(new fabric.Point(x, y), 'center', 'center'), finalLocalPoint = new fabric.Point(localPoint.x * scaleX + activeItem.pathOffset.x, localPoint.y * scaleY + activeItem.pathOffset.y);
        if (currentControl.pointIndex === 0) {
            path[0][1] = finalLocalPoint.x;
            path[0][2] = finalLocalPoint.y;
        }
        else if (currentControl.pointIsCurve) {
            if (editor && editor.shift) {
                path[currentControl.pointIndex][1] = path[currentControl.pointIndex][3];
                path[currentControl.pointIndex][2] = path[currentControl.pointIndex][4];
            }
            else {
                path[currentControl.pointIndex][1] = finalLocalPoint.x;
                path[currentControl.pointIndex][2] = finalLocalPoint.y;
            }
        }
        else {
            if ((path[currentControl.pointIndex][1] === path[currentControl.pointIndex][3])
                && (path[currentControl.pointIndex][2] === path[currentControl.pointIndex][4])) {
                path[currentControl.pointIndex][1] = finalLocalPoint.x;
                path[currentControl.pointIndex][2] = finalLocalPoint.y;
            }
            path[currentControl.pointIndex][3] = finalLocalPoint.x;
            path[currentControl.pointIndex][4] = finalLocalPoint.y;
        }
        const center = activeItem.getCenterPoint(), newPathObject = new fabric.Path(path);
        activeItem.set({
            path: newPathObject.path,
            width: newPathObject.width,
            height: newPathObject.height,
            dirty: true,
            pathOffset: newPathObject.pathOffset
        }).setCoords();
        activeItem.setPositionByOrigin(center, 'center', 'center');
        return true;
    }
    curvePositionHandler(dim, finalMatrix, fabricObject) {
        if (!(fabricObject instanceof fabric.Path)) {
            return new fabric.Point(0, 0);
        }
        const path = fabricObject.path;
        let x, y;
        if (this.pointIndex === 0) {
            x = path[0][1] - fabricObject.pathOffset.x;
            y = path[0][2] - fabricObject.pathOffset.y;
        }
        else if (this.pointIsCurve) {
            if ((path[this.pointIndex][1] === path[this.pointIndex][3]) && (path[this.pointIndex][2] === path[this.pointIndex][4])) {
                const previousPointX = this.pointIndex === 1 ? 1 : 3;
                const previousPointY = this.pointIndex === 1 ? 2 : 4;
                x = ((path[this.pointIndex - 1][previousPointX] + path[this.pointIndex][3]) / 2) - fabricObject.pathOffset.x;
                y = ((path[this.pointIndex - 1][previousPointY] + path[this.pointIndex][4]) / 2) - fabricObject.pathOffset.y;
            }
            else {
                x = path[this.pointIndex][1] - fabricObject.pathOffset.x;
                y = path[this.pointIndex][2] - fabricObject.pathOffset.y;
            }
        }
        else {
            x = path[this.pointIndex][3] - fabricObject.pathOffset.x;
            y = path[this.pointIndex][4] - fabricObject.pathOffset.y;
        }
        if (fabricObject.canvas
            && fabricObject.canvas.viewportTransform) {
            return fabric.util.transformPoint(new fabric.Point(x, y), fabric.util.multiplyTransformMatrices(fabricObject.canvas.viewportTransform, fabricObject.calcTransformMatrix()));
        }
        else {
            return fabric.util.transformPoint(new fabric.Point(x, y), fabricObject.calcTransformMatrix());
        }
    }
}
class RectangleDrawTool {
    constructor() {
        this.originX = 0;
        this.originY = 0;
        this.drawingMode = 2;
    }
    make(startX, startY, endX, endY, options, shift) {
        this.originX = startX;
        this.originY = startY;
        if (shift) {
            const size = Math.max(Math.abs(startX - endX), Math.abs(startY - endY));
            endX = startX > endX
                ? startX - size
                : startX + size;
            endY = startY > endY
                ? startY - size
                : startY + size;
        }
        return new fabric.Rect({
            left: endX < startX ? endX : startX,
            top: endY < startY ? endY : startY,
            width: Math.abs(startX - endX),
            height: Math.abs(startY - endY),
            ...options
        });
    }
    resize(object, x, y, ctl, alt, shift) {
        if (object instanceof fabric.Rect) {
            if (shift) {
                const size = Math.max(Math.abs(this.originX - x), Math.abs(this.originY - y));
                x = this.originX > x
                    ? this.originX - size
                    : this.originX + size;
                y = this.originY > y
                    ? this.originY - size
                    : this.originY + size;
            }
            object.set({
                originX: this.originX > x ? 'right' : 'left',
                originY: this.originY > y ? 'bottom' : 'top',
                width: Math.abs(this.originX - x),
                height: Math.abs(this.originY - y),
            }).setCoords();
        }
        return object;
    }
}
class TextDrawTool {
    constructor() {
        this.originX = 0;
        this.originY = 0;
        this.drawingMode = 4;
    }
    make(startX, startY, endX, endY, options, shift, font) {
        this.originX = startX;
        this.originY = startY;
        let sizeX = endX - startX;
        let sizeY = endY - startY;
        let size = 0;
        if (shift) {
            const absSizeX = Math.abs(sizeX);
            const absSizeY = Math.abs(sizeY);
            const xGreater = absSizeX >= absSizeY;
            const maxSize = xGreater ? sizeX : sizeY;
            if (Math.abs(absSizeX - absSizeY) <= 3) {
                if (xGreater) {
                    sizeX = maxSize;
                    sizeY = maxSize * (sizeX >= 0 !== sizeY >= 0 ? -1 : 1);
                }
                else {
                    sizeX = maxSize * (sizeX >= 0 !== sizeY >= 0 ? -1 : 1);
                    sizeY = maxSize;
                }
                size = Math.abs(Math.sqrt((maxSize * maxSize) * 2));
            }
            else if (xGreater) {
                sizeY = 0;
                size = Math.abs(maxSize);
            }
            else {
                sizeX = 0;
                size = Math.abs(maxSize);
            }
        }
        else {
            size = Math.abs(Math.sqrt((sizeX * sizeX) + (sizeY * sizeY)));
        }
        let angle = Math.atan2(sizeY, sizeX);
        angle *= 180 / Math.PI;
        angle -= 90;
        const opt = {
            left: endX < startX ? endX : startX,
            top: endY < startY ? endY : startY,
            fontSize: size,
            angle: angle,
            fill: options.fill || (!options.stroke || options.stroke === 'transparent' ? 'black' : options.stroke),
            stroke: options.stroke === 'transparent'
                && (!options.fill || options.fill === 'transparent') ? 'black' : options.stroke,
            strokeWidth: options.strokeWidth,
            strokeUniform: true,
            perPixelTargetFind: true,
            selectable: true,
        };
        if (font) {
            opt.fontFamily = font;
        }
        const text = new fabric.IText('', opt);
        text.controls['tqc'] = new fabric.Control({
            positionHandler: this.textCurvePositionHandler,
            actionHandler: this.textCurveActionHandler,
            actionName: 'modifyTextCurve'
        });
        text.enterEditing();
        return text;
    }
    resize(object, x, y, ctl, alt, shift) {
        if (object instanceof fabric.IText) {
            let sizeX = x - this.originX;
            let sizeY = y - this.originY;
            let size = 0;
            if (shift) {
                const absSizeX = Math.abs(sizeX);
                const absSizeY = Math.abs(sizeY);
                const xGreater = absSizeX >= absSizeY;
                const maxSize = xGreater ? sizeX : sizeY;
                if (Math.abs(absSizeX - absSizeY) <= 3) {
                    if (xGreater) {
                        sizeX = maxSize;
                        sizeY = maxSize * (sizeX >= 0 !== sizeY >= 0 ? -1 : 1);
                    }
                    else {
                        sizeX = maxSize * (sizeX >= 0 !== sizeY >= 0 ? -1 : 1);
                        sizeY = maxSize;
                    }
                    size = Math.abs(Math.sqrt((maxSize * maxSize) * 2));
                }
                else if (xGreater) {
                    sizeY = 0;
                    size = Math.abs(maxSize);
                }
                else {
                    sizeX = 0;
                    size = Math.abs(maxSize);
                }
            }
            else {
                size = Math.abs(Math.sqrt((sizeX * sizeX) + (sizeY * sizeY)));
            }
            let angle = Math.atan2(sizeY, sizeX);
            angle *= 180 / Math.PI;
            angle -= 90;
            object.set({
                fontSize: size,
                angle: angle,
            }).setCoords();
        }
        return object;
    }
    textCurveActionHandler(eventData, transform, x, y) {
        const activeItem = transform.target;
        if (!editor || !(activeItem instanceof fabric.IText)) {
            return false;
        }
        if (editor && editor.shift) {
            activeItem.curvePoint = { x: 0, y: 0 };
            activeItem.path = undefined;
            return activeItem.curvePoint
                && (activeItem.curvePoint.x !== 0
                    || activeItem.curvePoint.y !== 0);
        }
        const width = activeItem.width, rotateMatrix = activeItem._calcRotateMatrix(), translateMatrix = activeItem._calcTranslateMatrix(), vpt = activeItem.getViewportTransform(), startMatrix = fabric.util.multiplyTransformMatrices(vpt, translateMatrix);
        let finalMatrix = fabric.util.multiplyTransformMatrices(startMatrix, rotateMatrix);
        finalMatrix = fabric.util.multiplyTransformMatrices(finalMatrix, [1 / vpt[0], 0, 0, 1 / vpt[3], 0, 0]);
        const dim = activeItem._calculateCurrentDimensions(), pathLeft = fabric.util.transformPoint(new fabric.Point(-0.5 * dim.x, 0), finalMatrix), pathRight = fabric.util.transformPoint(new fabric.Point(0.5 * dim.x, 0), finalMatrix), path = [[
                'M',
                pathLeft.x, pathLeft.y
            ], [
                'Q',
                x, y,
                pathRight.x, pathRight.y
            ]], pathObject = new fabric.Path(path), curvePoint = fabric.util.transformPoint(new fabric.Point(x, y), fabric.util.invertTransform(finalMatrix)), scaledCurvePoint = { x: curvePoint.x / dim.x, y: curvePoint.y / dim.y };
        activeItem.curvePoint = scaledCurvePoint;
        editor.fabricCanvas.add(pathObject);
        pathObject.set({ width: width }).setCoords();
        activeItem.set({ path: pathObject });
        editor.fabricCanvas.remove(pathObject);
        return true;
    }
    textCurvePositionHandler(dim, finalMatrix, fabricObject) {
        if (!(fabricObject instanceof fabric.IText)) {
            return new fabric.Point(0, 0);
        }
        if (!fabricObject.curvePoint) {
            fabricObject.curvePoint = { x: 0, y: 0 };
        }
        return fabric.util.transformPoint(new fabric.Point(fabricObject.curvePoint.x * dim.x, fabricObject.curvePoint.y * dim.y), finalMatrix);
    }
}
class TriangleDrawTool {
    constructor() {
        this.originX = 0;
        this.originY = 0;
        this.drawingMode = 5;
    }
    make(startX, startY, endX, endY, options, shift) {
        this.originX = startX;
        this.originY = startY;
        if (shift) {
            const size = Math.max(Math.abs(startX - endX), Math.abs(startY - endY));
            endX = startX > endX
                ? startX - size
                : startX + size;
            endY = startY > endY
                ? startY - size
                : startY + size;
        }
        return new fabric.Triangle({
            left: endX < startX ? endX : startX,
            top: endY < startY ? endY : startY,
            width: Math.abs(startX - endX),
            height: Math.abs(startY - endY),
            ...options
        });
    }
    resize(object, x, y, ctl, alt, shift) {
        if (object instanceof fabric.Triangle) {
            if (shift) {
                const size = Math.max(Math.abs(this.originX - x), Math.abs(this.originY - y));
                x = this.originX > x
                    ? this.originX - size
                    : this.originX + size;
                y = this.originY > y
                    ? this.originY - size
                    : this.originY + size;
            }
            object.set({
                originX: this.originX > x ? 'right' : 'left',
                originY: this.originY > y ? 'bottom' : 'top',
                width: Math.abs(this.originX - x),
                height: Math.abs(this.originY - y),
            }).setCoords();
        }
        return object;
    }
}
class StateManager {
    constructor(fabricCanvas) {
        this.fabricCanvas = fabricCanvas;
        this._locked = false;
        this._maxCount = 100;
        this._redoStack = [];
        this._undoStack = [];
        this._currentState = fabricCanvas.toDatalessJSON();
    }
    redo(callback) {
        this.applyState(this._undoStack, this._redoStack.pop(), callback);
    }
    saveState() {
        if (this._locked) {
            return;
        }
        if (this._undoStack.length >= this._maxCount) {
            this._undoStack.shift();
        }
        this._undoStack.push(this._currentState);
        this._currentState = this.fabricCanvas.toDatalessJSON();
        this._redoStack = [];
    }
    undo(callback) {
        this.applyState(this._redoStack, this._undoStack.pop(), callback);
    }
    applyState(stack, state, callback) {
        if (!state) {
            return;
        }
        stack.push(this._currentState);
        this._currentState = state;
        const self = this;
        this._locked = true;
        this.fabricCanvas.loadFromJSON(this._currentState, function () {
            if (typeof callback === 'function') {
                callback();
            }
            self._locked = false;
        });
    }
}
class EditorClipboard {
    constructor(editor) {
        this.editor = editor;
    }
    copy(callback) {
        this.editor.fabricCanvas.getActiveObject().clone((object) => {
            this._clipObject = object;
            if (typeof (callback) === 'function') {
                callback();
            }
        });
    }
    cut() {
        this.copy(() => this.editor.deleteObjects());
    }
    paste() {
        if (!this._clipObject) {
            return;
        }
        this._clipObject.clone((clonedObject) => {
            this.editor.fabricCanvas.discardActiveObject();
            if (clonedObject.type === 'activeSelection') {
                clonedObject.canvas = this.editor.fabricCanvas;
                let self = this;
                clonedObject.forEachObject(function (obj) {
                    self.editor.fabricCanvas.add(obj);
                });
                clonedObject.setCoords();
            }
            else {
                this.editor.fabricCanvas.add(clonedObject);
            }
            this.editor.fabricCanvas.setActiveObject(clonedObject);
            this.editor.fabricCanvas.requestRenderAll();
            this.editor.stateManager.saveState();
        });
    }
}
class Editor {
    constructor(editorCanvas) {
        this._alt = false;
        this._ctl = false;
        this._shift = false;
        this._space = false;
        this._aspectRatio = 4 / 3;
        this._cursorMode = 0;
        this._enableRotation = true;
        this._lastX = 0;
        this._lastY = 0;
        this._listeningForKeys = true;
        this._mouseDown = false;
        this._moving = false;
        this._panning = false;
        this._redraw = false;
        this.editorCanvas = editorCanvas;
        this._drawTools = [
            new EllipseDrawTool(),
            new LineDrawTool(),
            new RectangleDrawTool(),
            new PathDrawTool(),
            new TextDrawTool(),
            new TriangleDrawTool(),
        ];
        this._drawTool = this._drawTools[1];
        this._drawToolOptions = {
            fill: '',
            shadow: '',
            stroke: 'black',
            strokeWidth: 1,
            perPixelTargetFind: true,
            selectable: true,
            strokeUniform: true,
        };
        this.fabricCanvas = new fabric.Canvas(this.editorCanvas, { selection: false });
        if (this.editorCanvas.parentElement
            && this.editorCanvas.parentElement.parentElement) {
            const bounds = this.editorCanvas.parentElement.parentElement.getBoundingClientRect();
            this.fabricCanvas.setDimensions({ width: bounds.width, height: Math.round(bounds.width / this._aspectRatio) });
        }
        this.initializeEvents();
        this.stateManager = new StateManager(this.fabricCanvas);
        this.clipboard = new EditorClipboard(this);
    }
    get alt() { return this._alt; }
    get ctl() { return this._ctl; }
    get shift() { return this._shift; }
    get space() { return this._space; }
    bringForward() {
        this.fabricCanvas.getActiveObject().bringForward(true);
    }
    bringToFront() {
        this.fabricCanvas.getActiveObject().bringToFront();
    }
    clear() {
        this.fabricCanvas.clear();
    }
    copy() {
        this.clipboard.copy();
    }
    cut() {
        this.clipboard.cut();
    }
    dispose() {
        window.removeEventListener("resize", this.resizeEvent);
        this.fabricCanvas.dispose();
    }
    deleteObjects() {
        this.fabricCanvas.remove(...this.fabricCanvas.getActiveObjects());
        this.fabricCanvas.discardActiveObject();
        this.saveState();
    }
    enableRotation(value) {
        if (this._enableRotation === value) {
            return;
        }
        this._enableRotation = value;
        this.fabricCanvas.forEachObject((obj) => {
            obj.setControlVisible('mtr', value);
        });
    }
    paste() {
        this.clipboard.paste();
    }
    redo() {
        this.stateManager.redo();
    }
    resizeEvent() {
        if (this._resizeTimeout) {
            clearTimeout(this._resizeTimeout);
        }
        const handler = () => {
            if (this.editorCanvas.parentElement
                && this.editorCanvas.parentElement.parentElement) {
                this.fabricCanvas.setDimensions({
                    width: "10px",
                    height: "10px",
                }, {
                    cssOnly: true
                });
                this.resizeEventInner();
            }
        };
        this._resizeTimeout = setTimeout(handler, 250);
    }
    sendBackwards() {
        this.fabricCanvas.getActiveObject().sendBackwards(true);
    }
    sendToBack() {
        this.fabricCanvas.getActiveObject().sendToBack();
    }
    setBackgroundImage(imageUrl) {
        this._backgroundUrl = imageUrl;
        if (imageUrl
            && this.editorCanvas.parentElement
            && this.editorCanvas.parentElement.parentElement) {
            const self = this;
            fabric.Image.fromURL(imageUrl, function (image) {
                if (!image || !image.width || !image.height) {
                    self.fabricCanvas.setBackgroundImage('', self.fabricCanvas.renderAll.bind(self.fabricCanvas));
                    return;
                }
                self._aspectRatio = image.width / image.height;
                if (self._borderWidth) {
                    self.fabricCanvas.setDimensions({
                        width: image.width + (self._borderWidth * 2),
                        height: image.height + (self._borderWidth * 2),
                    }, {
                        backstoreOnly: true
                    });
                    self.fabricCanvas.setBackgroundImage(image, self.fabricCanvas.renderAll.bind(self.fabricCanvas), {
                        top: self._borderWidth,
                        left: self._borderWidth,
                        originX: 'left',
                        originY: 'top',
                    });
                }
                else {
                    self.fabricCanvas.setDimensions({
                        width: image.width,
                        height: image.height
                    }, {
                        backstoreOnly: true
                    });
                    self.fabricCanvas.setBackgroundImage(image, self.fabricCanvas.renderAll.bind(self.fabricCanvas));
                }
                self.resizeEvent();
            });
        }
        else {
            this.fabricCanvas.setBackgroundImage('', this.fabricCanvas.renderAll.bind(this.fabricCanvas));
        }
        this.saveState();
    }
    setBorderColor(color) {
        if (this._borderColor !== color) {
            this._borderColor = color;
            if (this._borderWidth) {
                this.drawBorder();
            }
        }
    }
    setBorderPercent(value) {
        if (!value) {
            this.setBorderWidth(value);
            return;
        }
        const size = Math.min(this.fabricCanvas.width ?? 0, this.fabricCanvas.height ?? 0);
        if (size > 0) {
            this.setBorderWidth(size * value);
        }
    }
    setBorderWidth(value) {
        if (this._borderWidth !== value) {
            this._borderWidth = value;
            this.drawBorder();
            if (this._backgroundUrl) {
                this.setBackgroundImage(this._backgroundUrl);
            }
        }
    }
    setDrawingMode(mode) {
        if (mode === 6) {
            this.fabricCanvas.isDrawingMode = true;
        }
        else {
            this.fabricCanvas.isDrawingMode = false;
            this._drawTool = this._drawTools[mode];
        }
    }
    setFillColor(color) {
        if (color) {
            this._drawToolOptions.fill = color;
        }
        else {
            this._drawToolOptions.fill = '';
        }
        this.setToolOptions();
    }
    setFont(font) {
        if (!font) {
            return;
        }
        const self = this;
        WebFont.load({
            classes: false,
            google: {
                families: [font]
            },
            active: function () {
                self._font = font;
                const obj = self.fabricCanvas.getActiveObject();
                if (obj instanceof fabric.IText) {
                    obj.set({ fontFamily: font });
                }
            },
        });
    }
    setLineCap(cap) {
        this._drawToolOptions.strokeLineCap = cap;
        this.setToolOptions();
    }
    setRedraw(value) {
        this._redraw = value;
    }
    setStrokeColor(color) {
        if (color) {
            this._drawToolOptions.stroke = color;
            this.fabricCanvas.freeDrawingBrush.color = color;
        }
        else {
            this._drawToolOptions.stroke = 'transparent';
            this.fabricCanvas.freeDrawingBrush.color = 'black';
        }
        this.setToolOptions();
    }
    setStrokeDash1(value) {
        if (value) {
            const dash = value * (this._drawToolOptions.strokeWidth ?? 1);
            const other = this._drawToolOptions.strokeDashArray
                && this._drawToolOptions.strokeDashArray.length
                && this._drawToolOptions.strokeDashArray.length >= 3
                ? this._drawToolOptions.strokeDashArray[2]
                : dash;
            let space = Math.max((this._drawToolOptions.strokeWidth ?? 1), Math.min(dash, other) / 2);
            if (this._drawToolOptions.strokeLineCap === 'round') {
                space += (this._drawToolOptions.strokeWidth ?? 1);
            }
            this._drawToolOptions.strokeDashArray = [dash, space, other, space];
            this.fabricCanvas.freeDrawingBrush.strokeDashArray = [dash, space, other, space];
        }
        else {
            this._drawToolOptions.strokeDashArray = undefined;
            this.fabricCanvas.freeDrawingBrush.strokeDashArray = undefined;
        }
        this.setToolOptions();
    }
    setStrokeDash2(value) {
        if (value) {
            const dash = value * (this._drawToolOptions.strokeWidth ?? 1);
            const other = this._drawToolOptions.strokeDashArray
                && this._drawToolOptions.strokeDashArray.length
                && this._drawToolOptions.strokeDashArray.length >= 1
                ? this._drawToolOptions.strokeDashArray[0]
                : dash;
            let space = Math.max((this._drawToolOptions.strokeWidth ?? 1), Math.min(dash, other) / 2);
            if (this._drawToolOptions.strokeLineCap === 'round') {
                space += (this._drawToolOptions.strokeWidth ?? 1);
            }
            this._drawToolOptions.strokeDashArray = [other, space, dash, space];
            this.fabricCanvas.freeDrawingBrush.strokeDashArray = [other, space, dash, space];
        }
        else {
            this._drawToolOptions.strokeDashArray = undefined;
            this.fabricCanvas.freeDrawingBrush.strokeDashArray = undefined;
        }
        this.setToolOptions();
    }
    setStrokeWidth(value) {
        value ?? (value = 1);
        if (this._drawToolOptions.strokeWidth === value) {
            return;
        }
        if (this._drawToolOptions.strokeDashArray
            && this._drawToolOptions.strokeDashArray.length
            && this._drawToolOptions.strokeDashArray.length >= 3) {
            const dash1 = this._drawToolOptions.strokeDashArray[0] * value / (this._drawToolOptions.strokeWidth ?? 1);
            const dash2 = this._drawToolOptions.strokeDashArray[2] * value / (this._drawToolOptions.strokeWidth ?? 1);
            const space = Math.max((this._drawToolOptions.strokeWidth ?? 1), Math.min(dash1, dash2) / 2);
            this._drawToolOptions.strokeDashArray = [dash1, space, dash2, space];
        }
        this._drawToolOptions.strokeWidth = value;
        this.fabricCanvas.freeDrawingBrush.width = value;
        this.setToolOptions();
    }
    undo() {
        this.stateManager.undo();
    }
    constrainView() {
        const vpt = this.fabricCanvas.viewportTransform;
        const zoom = this.fabricCanvas.getZoom();
        if (!vpt) {
            return;
        }
        const width = this.fabricCanvas.getWidth();
        if (vpt[4] >= 0) {
            vpt[4] = 0;
        }
        else if (vpt[4] < width - width * zoom) {
            vpt[4] = width - width * zoom;
        }
        const height = this.fabricCanvas.getHeight();
        if (vpt[5] >= 0) {
            vpt[5] = 0;
        }
        else if (vpt[5] < height - height * zoom) {
            vpt[5] = height - height * zoom;
        }
    }
    drawBorder() {
        if (this._border) {
            this.fabricCanvas.remove(this._border);
        }
        if (!this._borderWidth) {
            return;
        }
        this._border = new fabric.Rect({
            left: 0,
            top: 0,
            width: this.fabricCanvas.getWidth() - this._borderWidth,
            height: this.fabricCanvas.getHeight() - this._borderWidth,
            fill: '',
            stroke: this._borderColor || 'white',
            strokeWidth: this._borderWidth,
            selectable: false,
            evented: false,
        });
        this.fabricCanvas.add(this._border);
        this.fabricCanvas.requestRenderAll();
    }
    initializeEvents() {
        this.fabricCanvas.on('mouse:down', (e) => {
            const _e = e.e;
            this._alt = _e.altKey;
            this._ctl = _e.ctrlKey;
            this._shift = _e.shiftKey;
            const pointer = this.fabricCanvas.getPointer(e.e);
            this.mouseDown(pointer.x, pointer.y);
        });
        this.fabricCanvas.on('mouse:move', (e) => {
            if (this._mouseDown) {
                const _e = e.e;
                this._alt = _e.altKey;
                this._ctl = _e.ctrlKey;
                this._shift = _e.shiftKey;
                const pointer = this.fabricCanvas.getPointer(e.e);
                this.mouseMove(pointer.x, pointer.y);
            }
        });
        this.fabricCanvas.on('mouse:up', (e) => {
            this._mouseDown = false;
            this.fabricCanvas.selection = true;
            if (this._panning && this.fabricCanvas.viewportTransform) {
                this.fabricCanvas.setViewportTransform(this.fabricCanvas.viewportTransform);
            }
            this._panning = false;
            if (this._createdObject instanceof fabric.IText) {
                this._cursorMode = 1;
                if (this._createdObject.hiddenTextarea) {
                    this._createdObject.hiddenTextarea.focus();
                }
            }
            if (this._moving) {
                if (this._createdObject) {
                    this.fabricCanvas.setActiveObject(this._createdObject);
                }
                this.saveState();
                this._moving = false;
            }
            this._createdObject = undefined;
        });
        this.fabricCanvas.on('mouse:wheel', (e) => {
            const _e = e.e;
            let zoom = Math.max(1, Math.min(20, this.fabricCanvas.getZoom() * (0.999 ** _e.deltaY)));
            this.fabricCanvas.zoomToPoint(new fabric.Point(_e.offsetX, _e.offsetY), zoom);
            _e.preventDefault();
            _e.stopPropagation();
            this.constrainView();
        });
        this.fabricCanvas.on('object:modified', (e) => {
            this.saveState();
        });
        this.fabricCanvas.on('selection:created', (e) => {
            this._cursorMode = 1;
        });
        this.fabricCanvas.on('selection:cleared', (e) => {
            this._cursorMode = 0;
        });
        this.fabricCanvas.on('text:editing:entered', (e) => {
            this._listeningForKeys = false;
            this._cursorMode = 1;
            const obj = e.target;
            if (obj instanceof fabric.IText
                && obj.curvePoint
                && (obj.curvePoint.x !== 0
                    || obj.curvePoint.y !== 0)) {
                obj.curvePoint = { x: 0, y: 0 };
                obj.path = undefined;
            }
        });
        this.fabricCanvas.on('text:editing:exited', (e) => {
            this._listeningForKeys = true;
            this._cursorMode = 0;
            if (e.target) {
                this.fabricCanvas.setActiveObject(e.target);
            }
            this.saveState();
        });
        window.addEventListener("resize", this.resizeEvent.bind(this));
        window.addEventListener("keydown", this.keyDownEvent.bind(this));
        window.addEventListener("keyup", this.keyUpEvent.bind(this));
    }
    keyDownEvent(ev) {
        if (!this._listeningForKeys) {
            return;
        }
        if (ev.key === " ") {
            this._space = true;
        }
        else if (ev.key === "Delete") {
            this.deleteObjects();
        }
        else if (ev.key === "Copy"
            || (ev.key === "c" && ev.ctrlKey)) {
            this.copy();
        }
        else if (ev.key === "Cut"
            || (ev.key === "x" && ev.ctrlKey)) {
            this.cut();
        }
        else if (ev.key === "Paste"
            || (ev.key === "v" && ev.ctrlKey)) {
            this.paste();
        }
        else if (ev.key === "Undo"
            || (ev.key === "z" && ev.ctrlKey)) {
            this.undo();
        }
        else if (ev.key === "Redo"
            || (ev.key === "y" && ev.ctrlKey)) {
            this.redo();
        }
    }
    keyUpEvent(ev) {
        if (ev.key === " ") {
            this._space = false;
        }
    }
    make(startX, startY, endX, endY) {
        if (this._redraw) {
            this.fabricCanvas.remove(...this.fabricCanvas.getObjects());
        }
        const obj = this._drawTool.make(startX, startY, endX, endY, this._drawToolOptions, this._shift, this._font);
        if (!this._enableRotation) {
            obj.setControlVisible('mtr', false);
        }
        this.fabricCanvas.add(obj);
        this.saveState();
        return obj;
    }
    mouseDown(x, y) {
        if (this._space) {
            this._mouseDown = true;
            this._panning = true;
            this.fabricCanvas.selection = false;
            this._lastX = x;
            this._lastY = y;
        }
        else if (this._alt) {
            const obj = this.fabricCanvas.getActiveObject();
            if (obj instanceof fabric.Path) {
                const tool = this._drawTools[3];
                this._mouseDown = true;
                this.fabricCanvas.selection = false;
                tool.addPoint(obj, x, y);
            }
        }
        else if (this._cursorMode === 0
            && !this.fabricCanvas.isDrawingMode
            && !this._alt
            && !this._ctl
            && this.fabricCanvas.getActiveObjects().length === 0) {
            this._mouseDown = true;
            this.fabricCanvas.selection = false;
            this._lastX = x;
            this._lastY = y;
        }
    }
    mouseMove(x, y) {
        if (this._panning) {
            const vpt = this.fabricCanvas.viewportTransform;
            const zoom = this.fabricCanvas.getZoom();
            if (vpt) {
                vpt[4] += (x - this._lastX) * zoom;
                vpt[5] += (y - this._lastY) * zoom;
                this.constrainView();
                this.fabricCanvas.requestRenderAll();
            }
            this._lastX = x;
            this._lastY = y;
        }
        else if (this._createdObject) {
            this._moving = true;
            this._drawTool.resize(this._createdObject, x, y, this._ctl, this._alt, this._shift);
            this.fabricCanvas.renderAll();
        }
        else {
            this._createdObject = this.make(this._lastX, this._lastY, x, y);
        }
    }
    resizeEventInner() {
        if (this._resizeTimeoutInner) {
            clearTimeout(this._resizeTimeoutInner);
        }
        const handler = () => {
            if (this.editorCanvas.parentElement
                && this.editorCanvas.parentElement.parentElement) {
                const bounds = this.editorCanvas.parentElement.parentElement.getBoundingClientRect();
                this.fabricCanvas.setDimensions({
                    width: `${bounds.width}px`,
                    height: `${Math.round(bounds.width / this._aspectRatio)}px`,
                }, {
                    cssOnly: true
                });
                this.drawBorder();
            }
        };
        this._resizeTimeoutInner = setTimeout(handler, 10);
    }
    saveState() {
        this.stateManager.saveState();
        this.fabricCanvas.renderAll();
    }
    setToolOptions() {
        this.fabricCanvas.getActiveObjects().forEach((obj) => {
            obj.set(this._drawToolOptions);
        });
        this.fabricCanvas.requestRenderAll();
    }
}
let editor;
export function bringForward() {
    if (editor) {
        editor.bringForward();
    }
}
export function bringToFront() {
    if (editor) {
        editor.bringToFront();
    }
}
export function clear() {
    if (editor) {
        editor.clear();
    }
}
export function copy() {
    if (editor) {
        editor.copy();
    }
}
export function cut() {
    if (editor) {
        editor.cut();
    }
}
export function deleteObjects() {
    if (editor) {
        editor.deleteObjects();
    }
}
export function dispose() {
    if (editor) {
        editor.dispose();
    }
}
export function enableRotation(value) {
    if (!editor) {
        return;
    }
    editor.enableRotation(value || false);
}
export function getJSON() {
    if (editor) {
        return JSON.stringify(editor.fabricCanvas);
    }
    return '';
}
export async function getObjUrl() {
    if (!editor) {
        return "";
    }
    const url = await new Promise(function (resolve) {
        if (editor) {
            editor.fabricCanvas.discardActiveObject().renderAll();
            editor.editorCanvas.toBlob((blob) => {
                if (blob) {
                    resolve(URL.createObjectURL(blob));
                }
                else {
                    resolve("");
                }
            });
        }
        else {
            resolve("");
        }
    });
    return url;
}
export function loadEditor(editorCanvas, imageUrl) {
    if (!editor) {
        editor = new Editor(editorCanvas);
    }
    else {
        editor.clear();
    }
    if (imageUrl) {
        editor.setBackgroundImage(imageUrl);
    }
}
export function loadJSON(json) {
    if (!editor) {
        return;
    }
    if (json) {
        editor.fabricCanvas.loadFromJSON(json, () => {
            const fonts = [];
            editor.fabricCanvas.forEachObject((obj) => {
                if (obj instanceof fabric.IText
                    && obj.fontFamily) {
                    fonts.push(obj.fontFamily);
                }
            });
            if (fonts.length) {
                WebFont.load({
                    classes: false,
                    google: {
                        families: fonts
                    },
                    active: function () {
                        editor.fabricCanvas.requestRenderAll();
                    },
                });
            }
        });
    }
}
export function paste() {
    if (editor) {
        editor.paste();
    }
}
export function redo() {
    if (editor) {
        editor.redo();
    }
}
export function resize() {
    if (editor) {
        editor.resizeEvent();
    }
}
export function revokeObjUrl(url) {
    URL.revokeObjectURL(url);
}
export function saveImage() {
    if (editor) {
        const link = document.createElement('a');
        link.setAttribute('download', 'image.png');
        link.setAttribute('href', editor.fabricCanvas.toDataURL({ format: 'image/png' }).replace('image/png', 'image/octet-stream'));
        link.click();
    }
}
export function setBackgroundImage(imageUrl) {
    if (!editor) {
        return;
    }
    editor.setBackgroundImage(imageUrl);
}
export function sendBackwards() {
    if (editor) {
        editor.sendBackwards();
    }
}
export function sendToBack() {
    if (editor) {
        editor.sendToBack();
    }
}
export function setBorderColor(color) {
    if (!editor) {
        return;
    }
    editor.setBorderColor(color);
}
export function setBorderPercent(value) {
    if (!editor) {
        return;
    }
    editor.setBorderPercent(value);
}
export function setBorderWidth(value) {
    if (!editor) {
        return;
    }
    editor.setBorderWidth(value);
}
export function setDrawingMode(mode) {
    if (!editor) {
        return;
    }
    editor.setDrawingMode(mode);
}
export function setFillColor(color) {
    if (!editor) {
        return;
    }
    editor.setFillColor(color);
}
export function setFont(font) {
    if (!font) {
        return;
    }
    if (!editor) {
        return;
    }
    editor.setFont(font);
}
export function setLineCap(cap) {
    if (!editor) {
        return;
    }
    editor.setLineCap(cap);
}
export function setRedraw(value) {
    if (!editor) {
        return;
    }
    editor.setRedraw(value || false);
}
export function setStrokeColor(color) {
    if (!editor) {
        return;
    }
    editor.setStrokeColor(color);
}
export function setStrokeDash1(value) {
    if (!editor) {
        return;
    }
    editor.setStrokeDash1(value);
}
export function setStrokeDash2(value) {
    if (!editor) {
        return;
    }
    editor.setStrokeDash2(value);
}
export function setStrokeWidth(value) {
    if (!editor) {
        return;
    }
    editor.setStrokeWidth(value);
}
export function undo() {
    if (editor) {
        editor.undo();
    }
}
//# sourceMappingURL=editor.js.map