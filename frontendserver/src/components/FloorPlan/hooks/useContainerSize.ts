import { useState, useEffect } from 'react';
import { ContainerSize } from '../types/floorPlan.types';

export const useContainerSize = (containerRef: React.RefObject<HTMLDivElement | null>) => {
  const [containerSize, setContainerSize] = useState<ContainerSize>({ width: 0, height: 0 });

  useEffect(() => {
    const updateSize = () => {
      if (containerRef.current) {
        setContainerSize({
          width: containerRef.current.clientWidth,
          height: containerRef.current.clientHeight
        });
      }
    };

    updateSize();
    window.addEventListener('resize', updateSize);
    return () => window.removeEventListener('resize', updateSize);
  }, [containerRef]);

  return containerSize;
};
