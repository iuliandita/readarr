import classNames from 'classnames';
import PropTypes from 'prop-types';
import React, { useCallback, useRef } from 'react';
import { useDrag, useDrop } from 'react-dnd';
import { QUALITY_PROFILE_ITEM } from 'Helpers/dragTypes';
import QualityProfileItem from './QualityProfileItem';
import QualityProfileItemGroup from './QualityProfileItemGroup';
import styles from './QualityProfileItemDragSource.css';

function QualityProfileItemDragSource(props) {
  const {
    editGroups,
    groupId,
    qualityId,
    name,
    allowed,
    items,
    qualityIndex,
    isDraggingUp,
    isDraggingDown,
    onCreateGroupPress,
    onDeleteGroupPress,
    onQualityProfileItemAllowedChange,
    onItemGroupAllowedChange,
    onItemGroupNameChange,
    onQualityProfileItemDragMove,
    onQualityProfileItemDragEnd
  } = props;

  const rootRef = useRef(null);

  const [{ isDragging }, connectDragSource] = useDrag({
    type: QUALITY_PROFILE_ITEM,
    item: () => ({
      editGroups: props.editGroups,
      qualityIndex: props.qualityIndex,
      groupId: props.groupId,
      qualityId: props.qualityId,
      isGroup: !props.qualityId,
      name: props.name,
      allowed: props.allowed
    }),
    collect: (monitor) => ({
      isDragging: monitor.isDragging()
    }),
    end: (item, monitor) => {
      onQualityProfileItemDragEnd(monitor.didDrop());
    }
  });

  const [{ isOverCurrent }, connectDropTarget] = useDrop({
    accept: QUALITY_PROFILE_ITEM,
    collect: (monitor) => ({
      isOverCurrent: monitor.isOver({ shallow: true })
    }),
    hover: (item, monitor) => {
      const {
        qualityIndex: dragQualityIndex,
        isGroup: isDragGroup
      } = item;

      const dropQualityIndex = qualityIndex;
      const isDropGroupItem = !!(qualityId && groupId);

      // Use childNodeIndex to select the correct node to get the middle of so
      // we don't bounce between above and below causing rapid setState calls.
      const childNodeIndex = isOverCurrent && isDraggingUp ? 1 : 0;
      const componentDOMNode = rootRef.current.children[childNodeIndex];
      const hoverBoundingRect = componentDOMNode.getBoundingClientRect();
      const hoverMiddleY = (hoverBoundingRect.bottom - hoverBoundingRect.top) / 2;
      const clientOffset = monitor.getClientOffset();
      const hoverClientY = clientOffset.y - hoverBoundingRect.top;

      // If we're hovering over a child don't trigger on the parent
      if (!monitor.isOver({ shallow: true })) {
        return;
      }

      // Don't show targets for dropping on self
      if (dragQualityIndex === dropQualityIndex) {
        return;
      }

      // Don't allow a group to be dropped inside a group
      if (isDragGroup && isDropGroupItem) {
        return;
      }

      let dropPosition = null;

      // Determine drop position based on position over target
      if (hoverClientY > hoverMiddleY) {
        dropPosition = 'below';
      } else if (hoverClientY < hoverMiddleY) {
        dropPosition = 'above';
      } else {
        return;
      }

      onQualityProfileItemDragMove({
        dragQualityIndex,
        dropQualityIndex,
        dropPosition
      });
    }
  });

  const setRootRef = useCallback((node) => {
    rootRef.current = node;
    connectDropTarget(node);
  }, [connectDropTarget]);

  const isBefore = !isDragging && isDraggingUp && isOverCurrent;
  const isAfter = !isDragging && isDraggingDown && isOverCurrent;

  return (
    <div
      ref={setRootRef}
      className={classNames(
        styles.qualityProfileItemDragSource,
        isBefore && styles.isDraggingUp,
        isAfter && styles.isDraggingDown
      )}
    >
      {
        isBefore &&
          <div
            className={classNames(
              styles.qualityProfileItemPlaceholder,
              styles.qualityProfileItemPlaceholderBefore
            )}
          />
      }

      {
        !!groupId && qualityId == null &&
          <QualityProfileItemGroup
            editGroups={editGroups}
            groupId={groupId}
            name={name}
            allowed={allowed}
            items={items}
            qualityIndex={qualityIndex}
            isDragging={isDragging}
            isDraggingUp={isDraggingUp}
            isDraggingDown={isDraggingDown}
            connectDragSource={connectDragSource}
            onDeleteGroupPress={onDeleteGroupPress}
            onQualityProfileItemAllowedChange={onQualityProfileItemAllowedChange}
            onItemGroupAllowedChange={onItemGroupAllowedChange}
            onItemGroupNameChange={onItemGroupNameChange}
            onQualityProfileItemDragMove={onQualityProfileItemDragMove}
            onQualityProfileItemDragEnd={onQualityProfileItemDragEnd}
          />
      }

      {
        qualityId != null &&
          <QualityProfileItem
            editGroups={editGroups}
            groupId={groupId}
            qualityId={qualityId}
            name={name}
            allowed={allowed}
            qualityIndex={qualityIndex}
            isDragging={isDragging}
            isOverCurrent={isOverCurrent}
            connectDragSource={connectDragSource}
            onCreateGroupPress={onCreateGroupPress}
            onQualityProfileItemAllowedChange={onQualityProfileItemAllowedChange}
          />
      }

      {
        isAfter &&
          <div
            className={classNames(
              styles.qualityProfileItemPlaceholder,
              styles.qualityProfileItemPlaceholderAfter
            )}
          />
      }
    </div>
  );
}

QualityProfileItemDragSource.propTypes = {
  editGroups: PropTypes.bool.isRequired,
  groupId: PropTypes.number,
  qualityId: PropTypes.number,
  name: PropTypes.string.isRequired,
  allowed: PropTypes.bool.isRequired,
  items: PropTypes.arrayOf(PropTypes.object),
  qualityIndex: PropTypes.string.isRequired,
  isDraggingUp: PropTypes.bool,
  isDraggingDown: PropTypes.bool,
  isInGroup: PropTypes.bool,
  onCreateGroupPress: PropTypes.func,
  onDeleteGroupPress: PropTypes.func,
  onQualityProfileItemAllowedChange: PropTypes.func.isRequired,
  onItemGroupAllowedChange: PropTypes.func,
  onItemGroupNameChange: PropTypes.func,
  onQualityProfileItemDragMove: PropTypes.func.isRequired,
  onQualityProfileItemDragEnd: PropTypes.func.isRequired
};

export default QualityProfileItemDragSource;
