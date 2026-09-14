import classNames from 'classnames';
import PropTypes from 'prop-types';
import React, { useCallback, useRef } from 'react';
import { useDrag, useDrop } from 'react-dnd';
import { DELAY_PROFILE } from 'Helpers/dragTypes';
import DelayProfile from './DelayProfile';
import styles from './DelayProfileDragSource.css';

function DelayProfileDragSource(props) {
  const {
    id,
    order,
    isDraggingUp,
    isDraggingDown,
    onDelayProfileDragMove,
    onDelayProfileDragEnd,
    ...otherProps
  } = props;

  const rootRef = useRef(null);

  const [{ isDragging }, connectDragSource] = useDrag({
    type: DELAY_PROFILE,
    item: () => props,
    collect: (monitor) => ({
      isDragging: monitor.isDragging()
    }),
    end: (item, monitor) => {
      onDelayProfileDragEnd(monitor.getItem(), monitor.didDrop());
    }
  });

  const [{ isOver }, connectDropTarget] = useDrop({
    accept: DELAY_PROFILE,
    collect: (monitor) => ({
      isOver: monitor.isOver()
    }),
    hover: (item, monitor) => {
      const dragIndex = item.order;
      const hoverIndex = order;

      const hoverBoundingRect = rootRef.current.getBoundingClientRect();
      const hoverMiddleY = (hoverBoundingRect.bottom - hoverBoundingRect.top) / 2;
      const clientOffset = monitor.getClientOffset();
      const hoverClientY = clientOffset.y - hoverBoundingRect.top;

      if (dragIndex === hoverIndex) {
        return;
      }

      // When moving up, only trigger if drag position is above 50% and
      // when moving down, only trigger if drag position is below 50%.
      // If we're moving down the hoverIndex needs to be increased
      // by one so it's ordered properly. Otherwise the hoverIndex will work.

      if (dragIndex < hoverIndex && hoverClientY > hoverMiddleY) {
        onDelayProfileDragMove(dragIndex, hoverIndex + 1);
      } else if (dragIndex > hoverIndex && hoverClientY < hoverMiddleY) {
        onDelayProfileDragMove(dragIndex, hoverIndex);
      }
    }
  });

  const setRootRef = useCallback((node) => {
    rootRef.current = node;
    connectDropTarget(node);
  }, [connectDropTarget]);

  const isBefore = !isDragging && isDraggingUp && isOver;
  const isAfter = !isDragging && isDraggingDown && isOver;

  return (
    <div
      ref={setRootRef}
      className={classNames(
        styles.delayProfileDragSource,
        isBefore && styles.isDraggingUp,
        isAfter && styles.isDraggingDown
      )}
    >
      {
        isBefore &&
          <div
            className={classNames(
              styles.delayProfilePlaceholder,
              styles.delayProfilePlaceholderBefore
            )}
          />
      }

      <DelayProfile
        id={id}
        order={order}
        isDragging={isDragging}
        isOver={isOver}
        {...otherProps}
        connectDragSource={connectDragSource}
      />

      {
        isAfter &&
          <div
            className={classNames(
              styles.delayProfilePlaceholder,
              styles.delayProfilePlaceholderAfter
            )}
          />
      }
    </div>
  );
}

DelayProfileDragSource.propTypes = {
  id: PropTypes.number.isRequired,
  order: PropTypes.number.isRequired,
  isDraggingUp: PropTypes.bool,
  isDraggingDown: PropTypes.bool,
  onDelayProfileDragMove: PropTypes.func.isRequired,
  onDelayProfileDragEnd: PropTypes.func.isRequired
};

export default DelayProfileDragSource;
