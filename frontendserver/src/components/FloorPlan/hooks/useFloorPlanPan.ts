import { useState, useCallback } from 'react';
import { ViewBox, Point, ContainerSize } from '../types/floorPlan.types.ts';

interface UseFloorPlanPanProps {
  viewBox: ViewBox;
  setViewBox: (viewBox: ViewBox) => void;
  containerSize: ContainerSize;
}

export const useFloorPlanPan = ({ viewBox, setViewBox, containerSize }: UseFloorPlanPanProps) => {
  const [isPanning, setIsPanning] = useState(false);
  const [panStart, setPanStart] = useState<Point>({ x: 0, y: 0 });

  const handleMouseDown = useCallback(
    (e: React.MouseEvent<SVGSVGElement>) => {
      if (e.button === 1 || e.button === 0) {
        // Middle or left mouse button
        setIsPanning(true);
        setPanStart({ x: e.clientX, y: e.clientY });
        e.preventDefault();
      }
    },
    []
  );

  const handleMouseMove = useCallback(
    (e: React.MouseEvent<SVGSVGElement>) => {
      if (isPanning) {
        const dx = (e.clientX - panStart.x) * (viewBox.width / containerSize.width);
        const dy = (e.clientY - panStart.y) * (viewBox.height / containerSize.height);

        setViewBox({
          ...viewBox,
          x: viewBox.x - dx,
          y: viewBox.y - dy
        });

        setPanStart({ x: e.clientX, y: e.clientY });
      }
    },
    [isPanning, panStart, viewBox, containerSize, setViewBox]
  );

  const handleMouseUp = useCallback(() => {
    setIsPanning(false);
  }, []);

  const handleMouseLeave = useCallback(() => {
    setIsPanning(false);
  }, []);

  // Touch support for mobile
  const handleTouchStart = useCallback(
    (e: React.TouchEvent<SVGSVGElement>) => {
      if (e.touches.length === 1) {
        setIsPanning(true);
        setPanStart({ x: e.touches[0].clientX, y: e.touches[0].clientY });
      }
    },
    []
  );

  const handleTouchMove = useCallback(
    (e: React.TouchEvent<SVGSVGElement>) => {
      if (isPanning && e.touches.length === 1) {
        const dx = (e.touches[0].clientX - panStart.x) * (viewBox.width / containerSize.width);
        const dy = (e.touches[0].clientY - panStart.y) * (viewBox.height / containerSize.height);

        setViewBox({
          ...viewBox,
          x: viewBox.x - dx,
          y: viewBox.y - dy
        });

        setPanStart({ x: e.touches[0].clientX, y: e.touches[0].clientY });
      }
    },
    [isPanning, panStart, viewBox, containerSize, setViewBox]
  );

  return {
    isPanning,
    handleMouseDown,
    handleMouseMove,
    handleMouseUp,
    handleMouseLeave,
    handleTouchStart,
    handleTouchMove
  };
};
