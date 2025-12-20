import { useState, useCallback, useEffect } from 'react';
import { ViewBox } from '../types/floorPlan.types.ts';
import { MIN_ZOOM_SCALE, MAX_ZOOM_SCALE, ZOOM_FACTOR } from '../config/constants.ts';

interface UseFloorPlanZoomProps {
  viewBox: ViewBox;
  setViewBox: (viewBox: ViewBox) => void;
  svgRef: React.RefObject<SVGSVGElement | null>;
}

export const useFloorPlanZoom = ({ viewBox, setViewBox, svgRef }: UseFloorPlanZoomProps) => {
  const [scale, setScale] = useState(1);

  const handleZoom = useCallback(
    (delta: number, clientX?: number, clientY?: number) => {
      const newScale = Math.max(MIN_ZOOM_SCALE, Math.min(MAX_ZOOM_SCALE, scale + delta * ZOOM_FACTOR));

      if (svgRef.current && clientX !== undefined && clientY !== undefined) {
        // Zoom towards cursor position
        const svg = svgRef.current;
        const pt = svg.createSVGPoint();
        pt.x = clientX;
        pt.y = clientY;

        const svgPoint = pt.matrixTransform(svg.getScreenCTM()?.inverse());

        const scaleRatio = newScale / scale;
        setViewBox({
          x: svgPoint.x - (svgPoint.x - viewBox.x) * scaleRatio,
          y: svgPoint.y - (svgPoint.y - viewBox.y) * scaleRatio,
          width: viewBox.width / scaleRatio,
          height: viewBox.height / scaleRatio
        });
      } else {
        // Zoom towards center
        setViewBox({
          x: viewBox.x + (viewBox.width * (1 - newScale / scale)) / 2,
          y: viewBox.y + (viewBox.height * (1 - newScale / scale)) / 2,
          width: viewBox.width / (newScale / scale),
          height: viewBox.height / (newScale / scale)
        });
      }

      setScale(newScale);
    },
    [scale, viewBox, setViewBox, svgRef]
  );

  // Use native event listener with passive: false to enable preventDefault
  useEffect(() => {
    const svg = svgRef.current;
    if (!svg) return;

    const handleWheel = (e: WheelEvent) => {
      e.preventDefault();
      const delta = -Math.sign(e.deltaY);
      handleZoom(delta, e.clientX, e.clientY);
    };

    svg.addEventListener('wheel', handleWheel, { passive: false });

    return () => {
      svg.removeEventListener('wheel', handleWheel);
    };
  }, [handleZoom, svgRef]);

  return {
    scale,
    setScale,
    handleZoom
  };
};
