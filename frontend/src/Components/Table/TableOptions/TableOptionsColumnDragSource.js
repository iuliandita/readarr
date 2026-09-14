import classNames from 'classnames';
import PropTypes from 'prop-types';
import React, { useCallback, useRef } from 'react';
import { useDrag, useDrop } from 'react-dnd';
import { TABLE_COLUMN } from 'Helpers/dragTypes';
import TableOptionsColumn from './TableOptionsColumn';
import styles from './TableOptionsColumnDragSource.css';

function TableOptionsColumnDragSource(props) {
  const {
    name,
    label,
    isVisible,
    isModifiable,
    index,
    isDraggingUp,
    isDraggingDown,
    onVisibleChange,
    onColumnDragMove,
    onColumnDragEnd
  } = props;

  const rootRef = useRef(null);

  const [{ isDragging }, connectDragSource] = useDrag({
    type: TABLE_COLUMN,
    item: () => props,
    collect: (monitor) => ({
      isDragging: monitor.isDragging()
    }),
    end: (item, monitor) => {
      onColumnDragEnd(monitor.getItem(), monitor.didDrop());
    }
  });

  const [{ isOver }, connectDropTarget] = useDrop({
    accept: TABLE_COLUMN,
    collect: (monitor) => ({
      isOver: monitor.isOver()
    }),
    hover: (item, monitor) => {
      const dragIndex = item.index;
      const hoverIndex = index;

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

      // Dragging downwards
      if (dragIndex < hoverIndex && hoverClientY < hoverMiddleY) {
        return;
      }

      // Dragging upwards
      if (dragIndex > hoverIndex && hoverClientY > hoverMiddleY) {
        return;
      }

      onColumnDragMove(dragIndex, hoverIndex);
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
        styles.columnDragSource,
        isBefore && styles.isDraggingUp,
        isAfter && styles.isDraggingDown
      )}
    >
      {
        isBefore &&
          <div
            className={classNames(
              styles.columnPlaceholder,
              styles.columnPlaceholderBefore
            )}
          />
      }

      <TableOptionsColumn
        name={name}
        label={typeof label === 'function' ? label() : label}
        isVisible={isVisible}
        isModifiable={isModifiable}
        index={index}
        isDragging={isDragging}
        isOver={isOver}
        connectDragSource={connectDragSource}
        onVisibleChange={onVisibleChange}
      />

      {
        isAfter &&
          <div
            className={classNames(
              styles.columnPlaceholder,
              styles.columnPlaceholderAfter
            )}
          />
      }
    </div>
  );
}

TableOptionsColumnDragSource.propTypes = {
  name: PropTypes.string.isRequired,
  label: PropTypes.oneOfType([PropTypes.string, PropTypes.func]).isRequired,
  isVisible: PropTypes.bool.isRequired,
  isModifiable: PropTypes.bool.isRequired,
  index: PropTypes.number.isRequired,
  isDraggingUp: PropTypes.bool,
  isDraggingDown: PropTypes.bool,
  onVisibleChange: PropTypes.func.isRequired,
  onColumnDragMove: PropTypes.func.isRequired,
  onColumnDragEnd: PropTypes.func.isRequired
};

export default TableOptionsColumnDragSource;
