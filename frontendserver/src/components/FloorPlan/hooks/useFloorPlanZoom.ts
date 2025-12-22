import { useState, useCallback, useEffect, useRef } from 'react';
import { ViewBox } from '../types/floorPlan.types.ts';
import { MIN_ZOOM_SCALE, MAX_ZOOM_SCALE, ZOOM_FACTOR } from '../config/constants.ts';

interface UseFloorPlanZoomProps {
  viewBox: ViewBox;
  setViewBox: (viewBox: ViewBox) => void;
  svgRef: React.RefObject<SVGSVGElement | null>;
}

export const useFloorPlanZoom = ({ viewBox, setViewBox, svgRef }: UseFloorPlanZoomProps) => {
  const [scale, setScale] = useState(1);
  const lastTouchDistance = useRef<number | null>(null);

  const handleZoom = useCallback(
    (delta: number, clientX?: number, clientY?: number) => {
      const newScale = Math.max(MIN_ZOOM_SCALE, Math.min(MAX_ZOOM_SCALE, scale + delta * ZOOM_FACTOR));
      const scaleFactor = scale / newScale;

      if (svgRef.current && clientX !== undefined && clientY !== undefined) {
        // Zoom towards cursor position
        const svg = svgRef.current;
        const rect = svg.getBoundingClientRect();

        // Convert client coordinates to SVG coordinates
        const x = (clientX - rect.left) / rect.width;
        const y = (clientY - rect.top) / rect.height;

        // Calculate the point in current viewBox coordinates
        const pointX = viewBox.x + viewBox.width * x;
        const pointY = viewBox.y + viewBox.height * y;

        // Calculate new viewBox dimensions
        const newWidth = viewBox.width * scaleFactor;
        const newHeight = viewBox.height * scaleFactor;

        // Calculate new viewBox position to keep the point under cursor
        const newX = pointX - newWidth * x;
        const newY = pointY - newHeight * y;

        setViewBox({
          x: newX,
          y: newY,
          width: newWidth,
          height: newHeight
        });
      } else {
        // Zoom towards center
        const newWidth = viewBox.width * scaleFactor;
        const newHeight = viewBox.height * scaleFactor;

        setViewBox({
          x: viewBox.x + (viewBox.width - newWidth) / 2,
          y: viewBox.y + (viewBox.height - newHeight) / 2,
          width: newWidth,
          height: newHeight
        });
      }

      setScale(newScale);
    },
    [scale, viewBox, setViewBox, svgRef]
  );

  // Pinch-to-zoom handler
  const handlePinchZoom = useCallback(
    (e: TouchEvent) => {
      if (e.touches.length === 2) {
        e.preventDefault();

        const touch1 = e.touches[0];
        const touch2 = e.touches[1];

        // Calculate distance between two touches
        const distance = Math.hypot(
          touch2.clientX - touch1.clientX,
          touch2.clientY - touch1.clientY
        );

        if (lastTouchDistance.current !== null) {
          // Calculate the center point between the two touches
          const centerX = (touch1.clientX + touch2.clientX) / 2;
          const centerY = (touch1.clientY + touch2.clientY) / 2;

          // Calculate zoom delta based on proportional distance change
          const distanceRatio = distance / lastTouchDistance.current;
          // Convert ratio to scale delta (more granular control)
          const scaleDelta = (distanceRatio - 1) * scale * 2;

          handleZoom(scaleDelta / ZOOM_FACTOR, centerX, centerY);
        }

        lastTouchDistance.current = distance;
      }
    },
    [handleZoom, scale]
  );

  const handleTouchEnd = useCallback(() => {
    lastTouchDistance.current = null;
  }, []);

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
    svg.addEventListener('touchmove', handlePinchZoom, { passive: false });
    svg.addEventListener('touchend', handleTouchEnd);

    return () => {
      svg.removeEventListener('wheel', handleWheel);
      svg.removeEventListener('touchmove', handlePinchZoom);
      svg.removeEventListener('touchend', handleTouchEnd);
    };
  }, [handleZoom, handlePinchZoom, handleTouchEnd, svgRef]);

  return {
    scale,
    setScale,
    handleZoom
  };
};
